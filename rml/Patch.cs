using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Renderite.Shared;

namespace Rebind
{
    public class Patch : ResoniteMod
    {
        public override string Author => "LeCloutPanda";
        public override string Name => "Rebind";
        public override string Version => "1.0.1";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"dev.lecloutpanda.rebind");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(TouchController))]
        class TouchControllerPatch
        {
            [HarmonyPatch("Bind")]
            [HarmonyPrefix]
            static bool Prefix(InputGroup group, TouchController __instance, Analog2D ___Joystick, Digital ___TriggerClick, Digital ___GripClick, Digital ___ButtonYB, Analog ___Trigger, Digital ___ButtonXA)
            {
                if (group.Side != __instance.Side)
                {
                    return false;
                }

                InteractionHandlerInputs commonToolInputs = group as InteractionHandlerInputs;
                if (commonToolInputs == null)
                {
                    LaserHoldInputs laserHoldInputs = group as LaserHoldInputs;
                    if (laserHoldInputs == null)
                    {
                        TeleportInputs teleportInputs = group as TeleportInputs;
                        if (teleportInputs == null)
                        {
                            GrabWorldInputs grabWorldInputs = group as GrabWorldInputs;
                            if (grabWorldInputs != null)
                            {
                                grabWorldInputs.Grab.AddBinding(InputNode.Analog2D(___Joystick).Y().ToDigital(0.5f), __instance);
                                grabWorldInputs.TurnDelta.AddBinding(InputNode.Analog2D(___Joystick).X().LocomotionTurn(), __instance);
                            }
                            else
                            {
                                //UniLog.Log($"Cannot bind {group.GetType()} to {GetType()}");
                            }
                        }
                        else
                        {
                            teleportInputs.Teleport.AddBinding(InputNode.Analog2D(___Joystick).Y().ToDigital(0.8f), __instance);
                            teleportInputs.Backstep.AddBinding(InputNode.Analog2D(___Joystick).Y().Negate().ToDigital(0.8f), __instance);
                            teleportInputs.TurnDelta.AddBinding(InputNode.Analog2D(___Joystick).X().LocomotionTurn(), __instance);
                        }
                    }
                    else
                    {
                        laserHoldInputs.Align.AddBinding(InputNode.Digital(___TriggerClick), __instance);
                        laserHoldInputs.Slide.AddBinding(InputNode.Analog2D(___Joystick).Y(), __instance);
                        laserHoldInputs.Rotate.AddBinding(InputNode.Analog2D(___Joystick).X(), __instance);
                    }
                }
                else
                {
                    commonToolInputs.Interact.AddBinding(InputNode.Digital(___TriggerClick), __instance);
                    commonToolInputs.Secondary.AddBinding(InputNode.Digital(___ButtonXA), __instance);
                    commonToolInputs.Grab.AddBinding(InputNode.Digital(___GripClick), __instance);
                    commonToolInputs.Menu.AddBinding(InputNode.Digital(___ButtonYB), __instance);
                    commonToolInputs.Strength.AddBinding(InputNode.Analog(___Trigger), __instance);
                    commonToolInputs.Axis.AddBinding(InputNode.Analog2D(___Joystick), __instance);
                }

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("BindNodeActions")]
            static bool PrefixBindActions(TouchController __instance, Digital ___ButtonXA, IInputNode node, string name = null)
            {
                IDualAxisInputNode dualAxisInputNode = node as IDualAxisInputNode;
                if (dualAxisInputNode == null)
                {
                    AnyInput anyInput = node as AnyInput;
                    if (anyInput == null)
                    {
                        LeftRightSelector<bool> leftRightSelector = node as LeftRightSelector<bool>;
                        if (leftRightSelector == null)
                        {
                            LeftRightSelector<float2> leftRightSelector2 = node as LeftRightSelector<float2>;
                            if (leftRightSelector2 == null)
                            {
                                SumInputs<float> sumInputs = node as SumInputs<float>;
                                if (sumInputs != null && name == "ReleaseStrength")
                                {
                                    sumInputs.Inputs.Add(InputNode.Analog2D(__instance.Side, "Joystick").Magnitude());
                                }
                            }
                            else if (name == "AnchorAxis")
                            {
                                leftRightSelector2.SetNode(__instance.Side, InputNode.Analog2D(__instance.Side, "Joystick"));
                            }
                        }
                        else
                        {
                            switch (name)
                            {
                                case "Jump":
                                case "Align":
                                    leftRightSelector.SetNode(__instance.Side, InputNode.Digital(__instance.Side, ___ButtonXA.Name));
                                    break;
                                case "AnchorAction":
                                    leftRightSelector.SetNode(__instance.Side, InputNode.Digital(__instance.Side, ___ButtonXA.Name));
                                    break;
                            }
                        }
                    }
                    else if (name == "Jump")
                    {
                        anyInput.Inputs.Add(InputNode.Digital(__instance.Side, ___ButtonXA.Name));
                    }
                }
                else
                {
                    dualAxisInputNode.SetInput(__instance.Side, InputNode.Analog2D(__instance.Side, "Joystick"));
                }

                return false;
            }
        }
    }
}
