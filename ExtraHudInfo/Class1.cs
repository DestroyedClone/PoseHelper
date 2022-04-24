using BepInEx;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using TMPro;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ExtraHudInfo
{
    [BepInPlugin("com.DestroyedClone.HudNumbahs", "Hud Numbahs", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            var ehicomp = self.gameObject.AddComponent<EHIComponent>();
            ehicomp.hud = self;
            ehicomp.characterMaster = self.targetMaster;
        }

        public class FadeOutDestroy : MonoBehaviour
        {
            public float age = 0;
            public float duration = 3f;

            public Vector3 relativeEndingPosition = Vector3.zero;
            private Vector3 startingPosition = Vector3.zero;
            private Vector3 endingPosition = Vector3.zero;

            public Color startingColor = Color.white;
            public Color endingColor = Color.black;

            public TextMeshProUGUI textMeshProUGUI;

            public void Start()
            {
                startingPosition = ((RectTransform)transform).position;
                endingPosition = startingPosition + relativeEndingPosition;
                startingColor = textMeshProUGUI.color;
            }

            public void Update()
            {
                age += Time.deltaTime;
                if (age >= duration)
                {
                    Destroy(gameObject);
                    return;
                }
                //float alpha = Mathf.Sqrt(Util.Remap(age, duration, 0f, 1f, 0f));
                float fraction = age / duration;
                float alpha = Mathf.Lerp(1, 0, fraction);
                textMeshProUGUI.alpha = alpha;
                transform.position = Vector3.Lerp(startingPosition, endingPosition, fraction);
                textMeshProUGUI.color = Color.Lerp(startingColor, endingColor, fraction);
            }
        }

        public class EHIComponent : MonoBehaviour
        {
            public HUD hud;

            public CharacterMaster characterMaster;

            public static GameObject displayPrefab;

            public LocalUser localUser = null;
            public UserProfile userProfile = null;

            //money
            public uint lastMoneyAmount = 0;

            public RectTransform moneyRoot;
            public RectTransform valueText;
            public List<int> moneyChanges = new List<int>();
            public readonly Vector3 localPosOffset = new Vector3(40, 0, 0);

            //lunar
            public uint lastCoinAmount = 0;

            public RectTransform coinRoot;
            public RectTransform valueTextLunar;
            public List<int> coinChanges = new List<int>();

            //health
            public HealthComponent healthComponent;

            public int lastHealth;
            public List<int> healthChanges = new List<int>();

            //shield
            public int lastShield;

            public List<int> shieldChanges = new List<int>();

            //barrier
            public int lastBarrier;

            public List<int> barrierChanges = new List<int>();

            //experience
            public CharacterBody characterBody;

            public int lastExperience = -1;
            public List<int> expChanges = new List<int>();
            public ExpBar expBar;

            public void Start()
            {
                AcquireIfMissing();
                //lastMoneyAmount = characterMaster.money;

                //moneyRoot = (RectTransform)hud.moneyText.transform;
                //valueText = hud.moneyText.targetText.rectTransform;
            }

            public void AcquireIfMissing()
            {
                if (!hud) hud = gameObject.GetComponent<HUD>();
                if (localUser == null) localUser = LocalUserManager.GetFirstLocalUser();
                if (userProfile == null) userProfile = localUser.userProfile;
                if (!characterMaster)
                {
                    characterMaster = localUser.cachedMaster;
                    if (characterMaster)
                        lastMoneyAmount = characterMaster.money;
                    lastCoinAmount = userProfile.coins;
                }
                if (!moneyRoot || !coinRoot)
                {
                    var moneyTexts = UnityEngine.GameObject.FindObjectsOfType<MoneyText>();
                    foreach (var a in moneyTexts)
                    {
                        if (a.name == "MoneyRoot")
                        {
                            moneyRoot = (RectTransform)a.transform;
                            valueText = a.targetText.rectTransform;
                        }
                        else if (a.name == "LunarCoinRoot")
                        {
                            coinRoot = (RectTransform)a.transform;
                            valueTextLunar = a.targetText.rectTransform;
                        }
                    }
                }
                if (!characterBody)
                {
                    if (characterMaster)
                        characterBody = characterMaster.GetBody();
                    if (characterBody)
                    {
                        if (!healthComponent)
                        {
                            healthComponent = characterBody.healthComponent;
                            lastHealth = (int)healthComponent.health;
                            lastBarrier = (int)healthComponent.barrier;
                            lastShield = (int)healthComponent.shield;
                        }
                    }
                }
                if (!expBar)
                {
                    expBar = FindObjectOfType<ExpBar>();
                }
                if (lastExperience == -1)
                {
                    if (characterBody)
                    {
                        lastExperience = (int)characterBody.experience;
                    }
                    /*if (TeamManager.instance)
                    {
                        var teamIndex = TeamIndex.Player;
                        lastExperience = (int)TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
                    }*/
                }
            }

            public void Update()
            {
                AcquireIfMissing();
                MoneyUpdate();
                CoinUpdate();
                HealthUpdate();
                ExperienceUpdate();
            }

            public void ExperienceUpdate()
            {
                if (characterBody)
                {
                    var experience = (int)characterBody.experience;
                    if (experience != lastExperience)
                    {
                        int difference = experience - lastExperience;
                        expChanges.Add(difference);
                        lastExperience = experience;
                    }
                }
                else if (TeamManager.instance)
                {
                    var teamIndex = TeamIndex.Player;
                    var experience = (int)TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
                    if (experience != lastExperience)
                    {
                        int difference = experience - lastExperience;
                        expChanges.Add(difference);
                        lastExperience = experience;
                    }
                }
                if (expChanges.Count != 0)
                    ShowExpChange();
            }

            public void ShowExpChange()
            {
                var value = expChanges[0];
                //Chat.AddMessage("Exp Change: " + value);

                var instance = UnityEngine.Object.Instantiate(valueTextLunar, (RectTransform)expBar.transform.parent);
                //instance.position = expBar.transform.position;
                instance.localPosition += new Vector3(40f, 0f, 0f);
                var tmp = instance.GetComponent<TextMeshProUGUI>();
                var isPos = Math.Sign(value) > 0;
                tmp.color = isPos ? new Color(56f / 255, 128f / 255, 63f / 255) : Color.red;
                tmp.text = (isPos ? "+" : "") + value.ToString();
                var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                dot.duration = 2f;
                dot.textMeshProUGUI = tmp;
                instance.name = "ExpChangeText";
                //Chat.AddMessage($"Exp Change: {tmp.text}");
                //dot.relativeEndingPosition = new Vector3(0, -20, 0);

                expChanges.RemoveAt(0);
            }

            public void HealthUpdate()
            {
                //health
                var health = (int)healthComponent.health;
                if (health != lastHealth)
                {
                    var healthDifference = health - lastHealth;
                    healthChanges.Add(healthDifference);
                    lastHealth = health;
                }

                //shield
                var shield = (int)healthComponent.shield;
                if (shield != lastShield)
                {
                    var shieldDifference = shield - lastShield;
                    shieldChanges.Add(shieldDifference);
                    lastShield = shield;
                }

                //barrier
                var barrier = (int)healthComponent.barrier;
                if (barrier != lastBarrier)
                {
                    var barrierDifference = barrier - lastBarrier;
                    barrierChanges.Add(barrierDifference);
                    lastBarrier = barrier;
                }
                ShowHealthbarChange(healthChanges.Count > 0, shieldChanges.Count > 0, barrierChanges.Count > 0);
            }

            public void ShowHealthbarChange(bool updateHealth, bool updateShield, bool updateBarrier)
            {
                if (updateHealth)
                {
                    var value = healthChanges[0];

                    var instance = UnityEngine.Object.Instantiate(valueTextLunar, hud.healthBar.rectTransform);
                    instance.position = hud.healthBar.rectTransform.position;
                    instance.localPosition += new Vector3(-200f, 20f, 0f);
                    var tmp = instance.GetComponent<TextMeshProUGUI>();
                    var isPos = Math.Sign(value) > 0;
                    tmp.color = isPos ? new Color(28f / 255, 212f / 255, 77f / 255) : Color.red;
                    tmp.text = (isPos ? "+" : "") + value.ToString();
                    var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                    dot.duration = 2f;
                    dot.textMeshProUGUI = tmp;
                    instance.name = "HealthChangeText";
                    dot.relativeEndingPosition = new Vector3(0, -2, 0);

                    healthChanges.RemoveAt(0);
                }
                if (updateShield)
                {
                    var value = shieldChanges[0];

                    var instance = UnityEngine.Object.Instantiate(valueTextLunar, hud.healthBar.rectTransform);
                    //instance.position = expBar.transform.position;
                    instance.localPosition += new Vector3(-20f, 20f, 0f);
                    var tmp = instance.GetComponent<TextMeshProUGUI>();
                    var isPos = Math.Sign(value) > 0;
                    tmp.color = isPos ? new Color(44f / 255, 137f / 255, 230f / 255) : new Color(197f / 255, 72f / 255, 217f / 255);
                    tmp.text = (isPos ? "+" : "") + value.ToString();
                    var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                    dot.duration = 2f;
                    dot.textMeshProUGUI = tmp;
                    instance.name = "ShieldChangeText";
                    dot.relativeEndingPosition = new Vector3(0, -2, 0);

                    shieldChanges.RemoveAt(0);
                }
                if (updateBarrier)
                {
                    var value = barrierChanges[0];

                    var instance = UnityEngine.Object.Instantiate(valueTextLunar, hud.healthBar.rectTransform);
                    //instance.position = expBar.transform.position;
                    instance.localPosition += new Vector3(-400f, 20f, 0f);
                    var tmp = instance.GetComponent<TextMeshProUGUI>();
                    var isPos = Math.Sign(value) > 0;
                    tmp.color = isPos ? new Color(191f / 255, 157f / 255, 46f / 255) : new Color(163f / 255, 60f / 255, 31f / 255);
                    tmp.text = (isPos ? "+" : "") + value.ToString();
                    var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                    dot.duration = 2f;
                    dot.textMeshProUGUI = tmp;
                    instance.name = "BarrierChangeText";
                    dot.relativeEndingPosition = new Vector3(0, -2, 0);

                    barrierChanges.RemoveAt(0);
                }
            }

            public void CoinUpdate()
            {
                if (userProfile.coins != lastCoinAmount)
                {
                    int difference = (int)userProfile.coins - (int)lastCoinAmount;
                    coinChanges.Add(difference);
                    lastCoinAmount = userProfile.coins;
                }
                if (coinChanges.Count == 0)
                    return;
                ShowCoinChange();
            }

            public void MoneyUpdate()
            {
                if (characterMaster.money != lastMoneyAmount)
                {
                    int difference = (int)characterMaster.money - (int)lastMoneyAmount;
                    moneyChanges.Add(difference);
                    lastMoneyAmount = characterMaster.money;
                }
                if (moneyChanges.Count == 0)
                    return;
                ShowMoneyChange();
            }

            public void ShowCoinChange()
            {
                var value = coinChanges[0];

                var instance = UnityEngine.Object.Instantiate(valueTextLunar, coinRoot);
                instance.position = valueTextLunar.position;
                instance.localPosition += localPosOffset;
                var tmp = instance.GetComponent<TextMeshProUGUI>();
                var isPos = Math.Sign(value) > 0;
                tmp.color = isPos ? new Color(85f / 255, 76f / 255, 186f / 255) : new Color(148f / 255, 44f / 255, 81f / 255);
                tmp.text = (isPos ? "+" : "") + value.ToString();
                var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                dot.duration = 1.5f;
                dot.textMeshProUGUI = tmp;
                dot.relativeEndingPosition = new Vector3(0, -2, 0);

                coinChanges.RemoveAt(0);
            }

            public void ShowMoneyChange()
            {
                var value = moneyChanges[0];

                var instance = UnityEngine.Object.Instantiate(valueText, moneyRoot);
                instance.position = valueText.position;
                instance.localPosition += localPosOffset;
                var tmp = instance.GetComponent<TextMeshProUGUI>();
                var isPos = Math.Sign(value) > 0;
                tmp.color = isPos ? new Color(152f / 255, 156f / 255, 50f / 255) : new Color(148f / 255, 65f / 255, 37f / 255);
                tmp.text = (isPos ? "+" : "") + value.ToString();
                var dot = instance.gameObject.AddComponent<FadeOutDestroy>();
                dot.duration = 1f;
                dot.textMeshProUGUI = tmp;
                dot.relativeEndingPosition = new Vector3(0, -2, 0);

                moneyChanges.RemoveAt(0);
            }
        }
    }
}