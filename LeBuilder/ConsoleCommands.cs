using System;
using System.Collections.Generic;
using System.Text;
using R2API;
using static R2API.Utils.CommandHelper;
using R2API.Utils;
using RoR2;
using UnityEngine;
using EntityStates;
using UnityEngine.Networking;

namespace LeBuilder
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Console Command")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Empty Arg required")]
    public static class ConsoleCommands
    {

        [ConCommand(commandName = "obj_enable", flags = ConVarFlags.ExecuteOnServer, helpText = "gives you the component to run the commands")]
        private static void PlayMinecraft(ConCommandArgs args)
        {
            var bodyObject = args.senderBody.gameObject;
            if (bodyObject)
            {
                var component = bodyObject.GetComponent<Minecraft>();
                if (!component)
                {
                    bodyObject.AddComponent<Minecraft>();
                    Debug.Log("Gave component!");
                    return;
                }
            }
        }

        [ConCommand(commandName = "obj_build", flags = ConVarFlags.ExecuteOnServer,
            helpText = "obj_build {objectname} {modelName} {opt:materialname} {opt:collisionname}")]
        private static void PlaceBlock(ConCommandArgs args)
        {
            var deathstate = args.senderBody.GetComponent<CharacterDeathBehavior>();
            if (deathstate) deathstate.deathState = new SerializableEntityStateType();
            args.senderMaster.preventGameOver = true;
        }

        [ConCommand(commandName = "obj_del", flags = ConVarFlags.ExecuteOnServer,
    helpText = "obj_del - deletes the currently selected object")]
        private static void DestroyBlock(ConCommandArgs args)
        {
            var minecraft = args.senderBody.gameObject.GetComponent<Minecraft>();
            if (minecraft)
            {
                if (minecraft.currentSelectedObject)
                {
                    UnityEngine.Object.DestroyImmediate(minecraft.currentSelectedObject.gameObject);
                }
            }
        }

        [ConCommand(commandName = "obj_ping", flags = ConVarFlags.ExecuteOnServer,
    helpText = "obj_ping - pings the selected object")]
        private static void FuckingPings(ConCommandArgs args)
        {
            var minecraft = args.senderBody.gameObject.GetComponent<Minecraft>();
            if (minecraft)
            {
                if (minecraft.currentSelectedObject)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/PositionIndicators/PoiPositionIndicator"), minecraft.currentSelectedObject.transform.position, minecraft.currentSelectedObject.transform.rotation);
                    PositionIndicator component = gameObject.GetComponent<PositionIndicator>();
                    component.insideViewObject.GetComponent<SpriteRenderer>().color = Color.white;
                    UnityEngine.Object.Destroy(component.insideViewObject.GetComponent<ObjectScaleCurve>());
                    component.insideViewObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("textures/miscicons/texAttackIcon");
                    component.outsideViewObject.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = Color.white;
                    component.outsideViewObject.transform.Find("Sprite").Find("Sprite").GetComponent<SpriteRenderer>().color = Color.white;
                    component.targetTransform = minecraft.currentSelectedObject.transform;
                    gameObject.AddComponent<ImpMarkerKiller>();
                }

            }
        }

        [ConCommand(commandName = "obj_sel", flags = ConVarFlags.ExecuteOnServer,
helpText = "obj_sel - look to select the nearest object")]
        private static void Scratchmyfuckingballs(ConCommandArgs args)
        {
            var minecraft = args.senderBody.gameObject.GetComponent<Minecraft>();
            if (minecraft)
            {
                InputBankTest component = args.senderBody.gameObject.GetComponent<InputBankTest>();
                if (component)
                {
                    if (Util.CharacterRaycast(args.senderBody.gameObject, new Ray(component.aimOrigin, component.aimDirection), out RaycastHit raycastHit, float.PositiveInfinity, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                    {
                        minecraft.currentSelectedObject = raycastHit.collider.gameObject;
                    }
                }
            }
        }

        //original code by evaisa
        public static void markGameObjects(HashSet<CharacterMaster> imps, Color impColor)
        {
            EnumerableExtensions.ForEachTry<CharacterMaster>(imps, delegate (CharacterMaster imp)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/PositionIndicators/PoiPositionIndicator"), imp.GetBodyObject().transform.position, imp.GetBodyObject().transform.rotation);
                PositionIndicator component = gameObject.GetComponent<PositionIndicator>();
                component.insideViewObject.GetComponent<SpriteRenderer>().color = impColor;
                UnityEngine.Object.Destroy(component.insideViewObject.GetComponent<ObjectScaleCurve>());
                component.insideViewObject.transform.localScale = component.insideViewObject.transform.localScale / 2f;
                component.insideViewObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("textures/miscicons/texAttackIcon");
                component.outsideViewObject.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = impColor;
                component.outsideViewObject.transform.Find("Sprite").Find("Sprite").GetComponent<SpriteRenderer>().color = impColor;
                component.targetTransform = imp.GetBodyObject().transform;
                gameObject.AddComponent<ImpMarkerKiller>();
            }, null);
        }

        public class ImpMarkerKiller : MonoBehaviour
        {
            float stopwatch = 0f;
            // Token: 0x0600001F RID: 31 RVA: 0x000033F0 File Offset: 0x000015F0
            public void Update()
            {
                stopwatch += Time.deltaTime;
                if (stopwatch > 5f || !base.GetComponent<PositionIndicator>().targetTransform)
                {
                    UnityEngine.Object.DestroyImmediate(base.gameObject);
                }
            }
        }

        [ConCommand(commandName = "obj_teleport_relative", flags = ConVarFlags.ExecuteOnServer,
    helpText = "obj_teleport_relative x y z")]
        private static void RELATIVEBITCH(ConCommandArgs args)
        {
            var minecraft = args.senderBody.gameObject.GetComponent<Minecraft>();
            if (minecraft)
            {
                if (minecraft.currentSelectedObject)
                {
                    minecraft.currentSelectedObject.transform.position += new Vector3(args.GetArgFloat(0), args.GetArgFloat(1), args.GetArgFloat(2));
                }

            }
        }

        [ConCommand(commandName = "obj_teleport_look", flags = ConVarFlags.ExecuteOnServer,
helpText = "obj_teleport_look")]
        private static void LOOKBITCH(ConCommandArgs args)
        {
            var minecraft = args.senderBody.gameObject.GetComponent<Minecraft>();
            if (minecraft)
            {
                if (minecraft.currentSelectedObject)
                {
                    InputBankTest component = args.senderBody.gameObject.GetComponent<InputBankTest>();
                    if (component)
                    {
                        if (Util.CharacterRaycast(args.senderBody.gameObject, new Ray(component.aimOrigin, component.aimDirection), out RaycastHit raycastHit, float.PositiveInfinity, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                        {
                            minecraft.currentSelectedObject.transform.position = raycastHit.transform.position;
                        }
                    }
                }

            }
        }


        [ConCommand(commandName = "past", flags = ConVarFlags.ExecuteOnServer,
            helpText = "past {acrid/commando/engineer/captain}")]
        private static void SelectPast(ConCommandArgs args)
        {
            var character = args.GetArgString(0).ToLower();

            switch (character)
            {
                case "acrid":
                    break;
                case "commando":
                    break;
                case "engineer":
                    break;
                case "captain":
                    break;
                default:
                    Debug.Log("No past data found for this name.");
                    break;
            }
        }


        public class Minecraft : MonoBehaviour
        {
            public GameObject currentSelectedObject = null;
        }
    }
}
