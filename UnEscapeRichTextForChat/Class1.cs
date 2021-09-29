using System;
using RoR2;
using BepInEx;

namespace UnEscapeRichTextForChat
{
    [BepInPlugin("com.DestroyedClone.RichTextForChat", "Rich Text For Chat", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.Util.EscapeRichTextForTextMeshPro += (On.RoR2.Util.orig_EscapeRichTextForTextMeshPro orig, string rtString) =>
            {
                return rtString;
            };
        }
    }
}
