/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Facepunch;
using Oxide.Core;
using Rust;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("No Cave Building", "VisEntities", "1.0.0")]
    [Description("Prevents players from building inside caves.")]
    public class NoCaveBuilding : RustPlugin
    {
        #region Fields

        private static NoCaveBuilding _plugin;

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _plugin = null;
        }

        private object CanBuild(Planner planner, Construction prefab, Construction.Target target)
        {
            if (planner == null || prefab == null)
                return null;

            BasePlayer player = planner.GetOwnerPlayer();
            if (player == null)
                return null;

            if (PermissionUtil.HasPermission(player, PermissionUtil.IGNORE))
                return null;

            if (InsideCave(target.position, 5f))
            {
                SendMessage(player, Lang.CannotBuildInCave);
                return true;
            }

            return null;
        }

        #endregion Oxide Hooks

        #region Permissions

        private static class PermissionUtil
        {
            public const string IGNORE = "nocavebuilding.ignore";
            private static readonly List<string> _permissions = new List<string>
            {
                IGNORE,
            };

            public static void RegisterPermissions()
            {
                foreach (var permission in _permissions)
                {
                    _plugin.permission.RegisterPermission(permission, _plugin);
                }
            }

            public static bool HasPermission(BasePlayer player, string permissionName)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Permissions
        
        #region Helper Functions

        public static bool InsideCave(Vector3 position, float radius)
        {
            List<Collider> colliders = Pool.Get<List<Collider>>();
            Vis.Colliders(position, radius, colliders, Layers.Mask.World, QueryTriggerInteraction.Ignore);
            
            bool result = false;

            foreach (Collider collider in colliders)
            {
                if (collider.name.Contains("cave", CompareOptions.OrdinalIgnoreCase))
                {
                    result = true;
                    break;
                }
            }

            Pool.FreeUnmanaged(ref colliders);
            return result;
        }

        #endregion Helper Functions

        #region Localization

        private class Lang
        {
            public const string CannotBuildInCave = "CannotBuildInCave";
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [Lang.CannotBuildInCave] = "You cannot build inside caves.",
            }, this, "en");
        }

        private void SendMessage(BasePlayer player, string messageKey, params object[] args)
        {
            string message = lang.GetMessage(messageKey, this, player.UserIDString);
            if (args.Length > 0)
                message = string.Format(message, args);

            SendReply(player, message);
        }

        #endregion Localization
    }
}