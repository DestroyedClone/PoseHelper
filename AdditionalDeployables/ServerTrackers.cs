using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using static AdditionalDeployables.Main;

namespace AdditionalDeployables
{
    public class ServerTrackers
    {
        public enum PerServerDeployableType
        {
            None,
            Gateway,
            Scanner //unused for now
        }

        private static bool CanDeploy(PerServerDeployableType perServerDeployableType)
        {
            if (ServerDeployableTracker.instance)
            {
                return ServerDeployableTracker.instance.CheckAvailibility(perServerDeployableType);
            }
            return false;
        }
        public class CustomDeployablePerServer : MonoBehaviour
        {
            public PerServerDeployableType deployableType = PerServerDeployableType.None;

            public void OnEnable()
            {
                if (ServerDeployableTracker.instance)
                {
                    ServerDeployableTracker.instance.AddItem(gameObject, deployableType);
                }
            }

            public void OnDisable()
            {
                if (ServerDeployableTracker.instance)
                {
                    ServerDeployableTracker.instance.RemoveItem(gameObject, deployableType);
                }
            }
        }
        public class ServerDeployableTracker : MonoBehaviour
        {
            public static ServerDeployableTracker instance;

            private List<GameObject> gatewayList = new List<GameObject>();

            private void OnEnable()
            {
                instance = this;
            }

            private void OnDisable()
            {
                instance = null;
            }

            public void AddItem(GameObject gameObject, PerServerDeployableType type)
            {
                if (!CheckAvailibility(type)) return;

                if (CheckAvailibility(type))
                    switch (type)
                    {
                    case PerServerDeployableType.Gateway:
                        AddGateway(gameObject);
                        break;
                    }
            }

            public void RemoveItem(GameObject gameObject, PerServerDeployableType type)
            {
                switch (type)
                {
                    case PerServerDeployableType.Gateway:
                        RemoveGateway(gameObject);
                        break;
                    case PerServerDeployableType.Scanner:
                        RemoveScanner(gameObject);
                        break;
                }
            }

            private void AddGateway(GameObject gameObject)
            {
                gatewayList.Add(gameObject);
            }

            private void RemoveGateway(GameObject gameObject)
            {
                gatewayList.Remove(gameObject);
            }

            public bool CheckAvailibility(PerServerDeployableType perServerDeployableType)
            {
                switch (perServerDeployableType)
                {
                    case PerServerDeployableType.Gateway:
                        return gatewayList.Count < cfgMaxGateway;
                }
                return false;
            }
        }
    }
}
