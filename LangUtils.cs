using System.Collections.Generic;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MechTransfer
{
    internal static class LangUtils
    {
        public static LocalizedText GetModTranslation(this Mod mod, string key)
        {
            IDictionary<string, LocalizedText> translations = (IDictionary<string, LocalizedText>)(typeof(Mod).GetField("translations", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mod));
            return translations[key];
        }

        public static void SwapTranslation(this LocalizedText target, LocalizedText source)
        {
            //Currently the Steam API supports 29 languages
            for (int i = 1; i <= 29; i++)
            {
                string translation = source.GetTranslation(i);
                if (i == 1 || translation != source.GetDefault())
                    target.AddTranslation(i, translation);
            }
        }
    }
}