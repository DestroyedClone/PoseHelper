
private void CharacterSelectController_Update(On.RoR2.UI.CharacterSelectController.orig_Update orig, CharacterSelectController self)
{
    self.SetEventSystem(self.eventSystemLocator.eventSystem);
    if (self.previousSurvivorIndex != self.selectedSurvivorIndex)
    {
        self.RebuildLocal();
        self.previousSurvivorIndex = self.selectedSurvivorIndex;
    }
    self.UpdateSurvivorInfoPanel();
    if (self.characterDisplayPads.Length != 0)
    {
        List<NetworkUser> sortedNetworkUsersList = self.GetSortedNetworkUsersList();
        for (int i = 0; i < self.characterDisplayPads.Length; i++)
        {
            ref CharacterSelectController.CharacterPad ptr = ref self.characterDisplayPads[i];
            NetworkUser networkUser = sortedNetworkUsersList[0];
            if (networkUser)
            {
                GameObject bodyPrefab = RoR2Content.Survivors.Merc.bodyPrefab; //BodyCatalog.GetBodyPrefab(networkUser.bodyIndexPreference);
                if (i < DelaySetup.debug_characters.Length && !DelaySetup.debug_characters[i].IsNullOrWhiteSpace()) //i < DelaySetup.instance.debug_characters.Length
                {
                    bodyPrefab = BodyCatalog.FindBodyPrefab(DelaySetup.debug_characters[i]);
                }
                else
                {
                    Debug.Log($"i {i} cocked");
                }

                SurvivorDef survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(bodyPrefab);
                if (survivorDef != null)
                {
                    SurvivorDef survivorDef2 = SurvivorCatalog.GetSurvivorDef(ptr.displaySurvivorIndex);
                    bool flag = true;
                    if (survivorDef2 != null) // && survivorDef2.bodyPrefab == bodyPrefab
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        GameObject displayPrefab = survivorDef.displayPrefab;
                        self.ClearPadDisplay(ptr);
                        if (!displayPrefab)
                        {
                            displayPrefab = RoR2Content.Items.ExtraLife.pickupModelPrefab;
                        }
                        if (displayPrefab)
                        {
                            ptr.displayInstance = UnityEngine.Object.Instantiate<GameObject>(displayPrefab, ptr.padTransform.position, ptr.padTransform.rotation, ptr.padTransform);
                            CharacterSelectSurvivorPreviewDisplayController component = ptr.displayInstance.GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
                            if (component)
                            {
                                //component.networkUser = networkUser;
                            }
                        }
                        ptr.displaySurvivorIndex = survivorDef.survivorIndex;
                        //self.OnNetworkUserLoadoutChanged(networkUser);
                    }
                }
                else
                {
                    self.ClearPadDisplay(ptr);
                }
            }
            else
            {
                self.ClearPadDisplay(ptr);
            }
            if (!ptr.padTransform)
            {
                return;
            }
            if (self.characterDisplayPads[i].padTransform)
            {
                //self.characterDisplayPads[i].padTransform.gameObject.SetActive(bodyPrefab != null);
                self.characterDisplayPads[i].padTransform.gameObject.SetActive(self.characterDisplayPads[i].displayInstance != null);
            }
        }
    }
    if (!RoR2Application.isInSinglePlayer)
    {
        bool flag2 = self.IsClientReady();
        self.readyButton.gameObject.SetActive(!flag2);
        self.unreadyButton.gameObject.SetActive(flag2);
    }
}


public string GetTeamName()
{
    CharacterSelectController self = gameObject.GetComponent<CharacterSelectController>();
    var networkUsers = self.GetSortedNetworkUsersList();

    List<string> boys = new List<string>();
    if (!debug)
    {
        foreach (var networkUser in networkUsers)
        {
            boys.Add(BodyCatalog.GetBodyName(networkUser.NetworkbodyIndexPreference));
        }
    }
    else
    {
        List<SurvivorIndex> chosenSurvivorIndices = new List<SurvivorIndex>();
        //KingEnderbrine code
        //var localUser = ((MPEventSystem)EventSystem.current).localUser;
        var currentIndex = self?.selectedSurvivorIndex ?? (SurvivorIndex)EclipseRun.cvEclipseSurvivorIndex.value;
        var survivors = SurvivorCatalog.orderedSurvivorDefs.Where(survivorDef => !survivorDef.hidden && SurvivorCatalog.SurvivorIsUnlockedOnThisClient(survivorDef.survivorIndex));

        for (int i = 0; i < 4; i++)
        {
            var randomIndex = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count())).survivorIndex;
            while (chosenSurvivorIndices.Contains(randomIndex))
            {
                randomIndex = survivors.ElementAt(UnityEngine.Random.Range(0, survivors.Count())).survivorIndex;
            }
            chosenSurvivorIndices.Add(randomIndex);
            var randomBodyIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(randomIndex);
            var randomBodyName = BodyCatalog.GetBodyName(randomBodyIndex);
            boys.Add(randomBodyName);
        }
    }
    Debug.Log("===");
    boys.Sort();
    foreach (var character in boys)
    {
        Debug.Log(character);
    }
    if (characterNames_to_teamName.TryGetValue(boys.ToArray(), out string output))
    {
        Debug.Log(output);
        return output;
    }
    else
    {
        return "None";
    }
}


public void Setup()
{
    CharacterSelectController self = gameObject.GetComponent<CharacterSelectController>();

    //self.transform.Find("SafeArea/RightHandPanel").gameObject.SetActive(false);
    Transform leftHandPanel = self.transform.Find("SafeArea/LeftHandPanel (Layer: Main)");
    leftHandPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
    leftHandPanel.Find("BorderImage").gameObject.SetActive(false);
    leftHandPanel.eulerAngles = Vector3.zero;
    Transform survivorChoiceGrid = leftHandPanel.Find("SurvivorChoiceGrid, Panel");
    survivorChoiceGrid.eulerAngles = Vector3.zero;
    survivorChoiceGrid.position = new Vector3(0f, -45f, 100f);
    Transform survivorInfoPanel = leftHandPanel.Find("SurvivorInfoPanel, Active (Layer: Secondary)");
    survivorInfoPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
    survivorInfoPanel.transform.position = new Vector3(30, 32.14879f, 100);
    //var snp = sip.Find("SurvivorNamePanel");
    //var shp = snp.Find("SubheaderPanel (Overview, Skills, Loadout)");
    //shp.eulerAngles = new Vector3(0, 0, 270);
    //shp.position = new Vector3(95, 27, 100);

    Transform rightHandPanel = self.transform.Find("SafeArea/RightHandPanel");
    rightHandPanel.Find("BlurPanel").gameObject.SetActive(false);
    rightHandPanel.Find("BorderImage").gameObject.SetActive(false);
    rightHandPanel.Find("RuleBookViewerVertical").GetComponent<Image>().enabled = false;

    int index = 0;
    foreach (Transform child in rightHandPanel.Find("/Viewport/Content"))
    {
        if (child.name != "RulebookCategoryPrefab(Clone)")
            continue;
        if (index == 0)
        {
            child.Find("Header").gameObject.SetActive(false);
            index++;
        }
        else if (index == 1)
        {
            child.Find("VoteResultGridContainer").gameObject.SetActive(false);
            break;
        }
    }
    foreach (Transform child in rightHandPanel.Find("PopoutPanelContainer"))
    {
        if (child.name == "PopoutPanelPrefab(Clone)")
        {
            if (child.Find("Main/Title and Subtitle/Title Text").GetComponent<LanguageTextMeshController>().token == "RULE_HEADER_ARTIFACTS")
            {
                child.Find("Main/Title and Subtitle").gameObject.SetActive(false);
                break;
            }
        }
    }
}
