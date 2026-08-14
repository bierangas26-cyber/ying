using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Patch
{
    public static class TimeStopManager
    {
        public static bool IsActive { get; private set; }

        public static ulong? PendingStasisRetrievalPlayerId { get; set; }

        private static readonly List<DelayedDamage> _damageQueue = new();

        private static ColorRect? _blueOverlay;
        private static Tween? _overlayTween;

        public sealed class DelayedDamage
        {
            public PlayerChoiceContext choiceContext;
            public List<Creature> targets;
            public decimal amount;
            public ValueProp props;
            public Creature dealer;
            public CardModel cardSource;
        }

        public static void StartTimeStop()
        {
            if (IsActive)
            {
                Log.Info("[TimeStop] StartTimeStop SKIPPED (already active)");
                return;
            }
            IsActive = true;
            Log.Info("[TimeStop] StartTimeStop ACTIVATED");

            PauseAllCreatureAnimations();
            StartBlueVisualEffect();
            _damageQueue.Clear();
        }

        public static async Task EndTimeStop()
        {
            if (!IsActive)
            {
                Log.Info("[TimeStop] EndTimeStop SKIPPED (not active)");
                return;
            }
            IsActive = false;
            Log.Info($"[TimeStop] EndTimeStop STARTED, queue count = {_damageQueue.Count}");

            ResumeAllCreatureAnimations();
            StopBlueVisualEffect();
            CleanupNullOwnerPowers();

            for (int i = _damageQueue.Count - 1; i >= 0; i--)
            {
                var damage = _damageQueue[i];
                var aliveTargets = damage.targets
                    .Where(t => t != null && !t.IsDead && t.CombatState != null)
                    .ToList();

                if (aliveTargets.Count == 0)
                {
                    var fallbackTargets = damage.dealer?.CombatState?.HittableEnemies?.ToList() ?? new();
                    if (fallbackTargets.Count > 0)
                    {
                        var rng = damage.dealer.CombatState.RunState.Rng.CombatTargets;
                        var fallback = rng.NextItem(fallbackTargets);
                        aliveTargets.Add(fallback);
                    }
                    else
                    {
                        Log.Info($"[TimeStop] Skipping damage: NO TARGET, card={damage.cardSource?.Id.Entry}");
                        _damageQueue.RemoveAt(i);
                        continue;
                    }
                }

                Log.Info($"[TimeStop] Resolving damage: card={damage.cardSource?.Id.Entry}, amount={damage.amount}, targets={aliveTargets.Count}");
                await CreatureCmd.Damage(
                    damage.choiceContext,
                    aliveTargets,
                    damage.amount,
                    damage.props,
                    damage.dealer,
                    damage.cardSource
                );

                _damageQueue.RemoveAt(i);
            }

            _damageQueue.Clear();
            Log.Info("[TimeStop] EndTimeStop COMPLETED");
        }

        public static void RecordDamage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (!IsActive) return;

            var list = targets.ToList();
            Log.Info($"[RecordDamage] Card={cardSource?.Id.Entry}, amount={amount}, targets={list.Count}");
            _damageQueue.Add(new DelayedDamage
            {
                choiceContext = choiceContext,
                targets = list,
                amount = amount,
                props = props,
                dealer = dealer,
                cardSource = cardSource
            });
        }

        private static void CleanupNullOwnerPowers()
        {
            var room = NCombatRoom.Instance;
            if (room == null) return;

            foreach (var node in room.CreatureNodes)
            {
                if (node == null || !GodotObject.IsInstanceValid(node)) continue;
                var creature = node.Entity;
                if (creature == null) continue;

                var nullOwnerPowers = creature.Powers.Where(p => p.Owner == null).ToList();
                foreach (var power in nullOwnerPowers)
                {
                    Log.Error($"[TimeStop] Removing null-owner power: {power.Id}");
                    creature.RemovePowerInternal(power);
                }
            }
        }

        private static void StartBlueVisualEffect()
        {
            var combatRoom = NCombatRoom.Instance;
            if (combatRoom == null) return;

            if (_blueOverlay == null || !GodotObject.IsInstanceValid(_blueOverlay))
            {
                _blueOverlay = new ColorRect
                {
                    Color = new Color(0.75f, 0.85f, 1f, 0f),
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    LayoutMode = 1,
                    AnchorLeft = 0f,
                    AnchorTop = 0f,
                    AnchorRight = 1f,
                    AnchorBottom = 1f,
                    ZIndex = 100
                };
                combatRoom.AddChild(_blueOverlay);
            }

            _overlayTween?.Kill();
            _overlayTween = _blueOverlay.CreateTween();
            _overlayTween.TweenProperty(_blueOverlay, "color:a", 0.18f, 2.0f)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Cubic);
        }

        private static void StopBlueVisualEffect()
        {
            if (_blueOverlay == null || !GodotObject.IsInstanceValid(_blueOverlay)) return;

            _overlayTween?.Kill();
            _overlayTween = _blueOverlay.CreateTween();
            _overlayTween.TweenProperty(_blueOverlay, "color:a", 0f, 0.8f)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Cubic);
            _overlayTween.TweenCallback(Callable.From(() =>
            {
                if (_blueOverlay != null && GodotObject.IsInstanceValid(_blueOverlay))
                {
                    _blueOverlay.QueueFree();
                    _blueOverlay = null;
                }
            }));
        }

        private static void PauseAllCreatureAnimations()
        {
            var room = NCombatRoom.Instance;
            if (room == null) return;

            foreach (var node in room.CreatureNodes)
            {
                if (node == null || !GodotObject.IsInstanceValid(node)) continue;
                if (node.Entity.IsEnemy && node.HasSpineAnimation)
                    node.SpineAnimation.SetTimeScale(0f);
            }
        }

        private static void ResumeAllCreatureAnimations()
        {
            var room = NCombatRoom.Instance;
            if (room == null) return;

            foreach (var node in room.CreatureNodes)
            {
                if (node == null || !GodotObject.IsInstanceValid(node)) continue;
                if (node.Entity.IsEnemy && node.HasSpineAnimation)
                    node.SpineAnimation.SetTimeScale(1f);
            }
        }
    }
}