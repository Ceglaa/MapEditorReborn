﻿namespace MapEditorReborn
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using API;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.CustomItems.API.Features;
    using Exiled.Loader;
    using MEC;
    using Mirror;
    using Mirror.LiteNetLib4Mirror;
    using RemoteAdmin;
    using UnityEngine;

    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Contains mostly methods for spawning objects, saving/loading maps.
    /// </summary>
    public static partial class Handler
    {
        #region Map Schematic Methods

        /// <summary>
        /// Loads the <see cref="MapSchematic"/> map.
        /// It also may be used for reloading the map.
        /// </summary>
        /// <param name="map"><see cref="MapSchematic"/> to load.</param>
        public static void LoadMap(MapSchematic map)
        {
            Log.Debug("Trying to load the map...", Config.Debug);

            foreach (MapEditorObject mapEditorObject in SpawnedObjects)
            {
                mapEditorObject.Destroy();
            }

            SpawnedObjects.Clear();

            Log.Debug("Destroyed all map's GameObjects and indicators.", Config.Debug);

            if (map == null)
            {
                Log.Debug("Map is null. Returning...", Config.Debug);
                return;
            }

            // Map.Rooms is null at this time, so this delay is required.
            Timing.CallDelayed(0.01f, () =>
            {
                // This MUST be executed first. If the default spawnpoins were destroyed I only have a brief period of time to replace them with a new ones.
                foreach (PlayerSpawnPointObject playerSpawnPoint in map.PlayerSpawnPoints)
                {
                    Log.Debug($"Trying to spawn a player spawn point at {playerSpawnPoint.Position}...", Config.Debug);
                    SpawnPlayerSpawnPoint(playerSpawnPoint);
                }

                if (map.PlayerSpawnPoints.Count > 0)
                    Log.Debug("All player spawn points have been spawned!", Config.Debug);

                foreach (DoorObject door in map.Doors)
                {
                    Log.Debug($"Trying to spawn door at {door.Position}...", Config.Debug);
                    SpawnDoor(door);
                }

                if (map.Doors.Count > 0)
                    Log.Debug("All doors have been successfully spawned!", Config.Debug);

                foreach (WorkStationObject workstation in map.WorkStations)
                {
                    Log.Debug($"Spawning workstation at {workstation.Position}...", Config.Debug);
                    SpawnWorkStation(workstation);
                }

                if (map.WorkStations.Count > 0)
                    Log.Debug("All workstations have been successfully spawned!", Config.Debug);

                foreach (ItemSpawnPointObject itemSpawnPoint in map.ItemSpawnPoints)
                {
                    Log.Debug($"Trying to spawn a item spawn point at {itemSpawnPoint.Position}...", Config.Debug);
                    SpawnItemSpawnPoint(itemSpawnPoint);
                }

                if (map.ItemSpawnPoints.Count > 0)
                    Log.Debug("All item spawn points have been spawned!", Config.Debug);

                foreach (RagdollSpawnPointObject ragdollSpawnPoint in map.RagdollSpawnPoints)
                {
                    Log.Debug($"Trying to spawn a ragdoll spawn point at {ragdollSpawnPoint.Position}...", Config.Debug);
                    SpawnRagdollSpawnPoint(ragdollSpawnPoint);
                }

                if (map.RagdollSpawnPoints.Count > 0)
                    Log.Debug("All ragdoll spawn points have been spawned!", Config.Debug);

                foreach (ShootingTargetObject shootingTargetObject in map.ShootingTargetObjects)
                {
                    Log.Debug($"Trying to spawn a shooting target at {shootingTargetObject.Position}...", Config.Debug);
                    SpawnShootingTarget(shootingTargetObject);
                }

                if (map.ShootingTargetObjects.Count > 0)
                    Log.Debug("All shooting targets have been spawned!", Config.Debug);

                foreach (LightControllerObject lightControllerObject in map.LightControllerObjects)
                {
                    Log.Debug($"Trying to spawn a light controller at {lightControllerObject.RoomType}...", Config.Debug);
                    SpawnLightController(lightControllerObject);
                }

                if (map.LightControllerObjects.Count > 0)
                    Log.Debug("All light controllers have been spawned!", Config.Debug);

                foreach (TeleportObject teleportObject in map.TeleportObjects)
                {
                    Log.Debug($"Trying to spawn a teleporter at {teleportObject.EntranceTeleporterPosition}...", Config.Debug);
                    SpawnTeleport(teleportObject);
                }

                if (map.TeleportObjects.Count > 0)
                    Log.Debug("All teleporters have been spawned!", Config.Debug);

                Log.Debug("All GameObject have been spawned and the MapSchematic has been fully loaded!", Config.Debug);
            });
        }

        /// <summary>
        /// Saves the map to a file.
        /// </summary>
        /// <param name="name">The name of the map.</param>
        public static void SaveMap(string name)
        {
            Log.Debug("Trying to save the map...", Config.Debug);

            MapSchematic map = GetMapByName(name);

            if (map == null)
            {
                map = new MapSchematic(name);
            }
            else
            {
                map.CleanupAll();
            }

            Log.Debug($"Map name set to \"{map.Name}\"", Config.Debug);

            foreach (MapEditorObject spawnedObject in SpawnedObjects)
            {
                if (spawnedObject is IndicatorObjectComponent)
                    continue;

                Log.Debug($"Trying to save GameObject at {spawnedObject.transform.position}...", Config.Debug);

                switch (spawnedObject)
                {
                    case DoorObjectComponent door:
                        {
                            door.Base.Position = door.RelativePosition;
                            door.Base.Rotation = door.RelativeRotation;
                            door.Base.Scale = door.Scale;
                            door.Base.RoomType = door.RoomType;

                            map.Doors.Add(door.Base);

                            break;
                        }

                    case WorkStationObjectComponent workStation:
                        {
                            workStation.Base.Position = workStation.RelativePosition;
                            workStation.Base.Rotation = workStation.RelativeRotation;
                            workStation.Base.Scale = workStation.Scale;
                            workStation.Base.RoomType = workStation.RoomType;

                            map.WorkStations.Add(workStation.Base);

                            break;
                        }

                    case PlayerSpawnPointComponent playerspawnPoint:
                        {
                            playerspawnPoint.Base.Position = playerspawnPoint.RelativePosition;
                            playerspawnPoint.Base.RoomType = playerspawnPoint.RoomType;

                            map.PlayerSpawnPoints.Add(playerspawnPoint.Base);

                            break;
                        }

                    case ItemSpawnPointComponent itemSpawnPoint:
                        {
                            itemSpawnPoint.Base.Position = itemSpawnPoint.RelativePosition;
                            itemSpawnPoint.Base.Rotation = itemSpawnPoint.RelativeRotation;
                            itemSpawnPoint.Base.RoomType = itemSpawnPoint.RoomType;

                            map.ItemSpawnPoints.Add(itemSpawnPoint.Base);

                            break;
                        }

                    case RagdollSpawnPointComponent ragdollSpawnPoint:
                        {
                            ragdollSpawnPoint.Base.Position = ragdollSpawnPoint.RelativePosition;
                            ragdollSpawnPoint.Base.Rotation = ragdollSpawnPoint.RelativeRotation;
                            ragdollSpawnPoint.Base.RoomType = ragdollSpawnPoint.RoomType;

                            map.RagdollSpawnPoints.Add(ragdollSpawnPoint.Base);

                            break;
                        }

                    case ShootingTargetComponent shootingTarget:
                        {
                            shootingTarget.Base.Position = shootingTarget.RelativePosition;
                            shootingTarget.Base.Rotation = shootingTarget.RelativeRotation;
                            shootingTarget.Base.Scale = shootingTarget.Scale;
                            shootingTarget.Base.RoomType = shootingTarget.RoomType;

                            map.ShootingTargetObjects.Add(shootingTarget.Base);

                            break;
                        }

                    case LightControllerComponent lightController:
                        {
                            map.LightControllerObjects.Add(lightController.Base);

                            break;
                        }

                    case TeleportControllerComponent teleportController:
                        {
                            teleportController.Base.EntranceTeleporterPosition = teleportController.EntranceTeleport.RelativePosition;
                            teleportController.Base.EntranceTeleporterRoomType = teleportController.EntranceTeleport.RoomType;
                            teleportController.Base.ExitTeleporterPosition = teleportController.ExitTeleport.RelativePosition;
                            teleportController.Base.ExitTeleporterRoomType = teleportController.ExitTeleport.RoomType;

                            map.TeleportObjects.Add(teleportController.Base);

                            break;
                        }
                }
            }

            string path = Path.Combine(MapEditorReborn.PluginDir, $"{map.Name}.yml");

            Log.Debug($"Path to file set to: {path}", Config.Debug);

            bool prevValue = Config.EnableFileSystemWatcher;
            if (prevValue)
                Config.EnableFileSystemWatcher = false;

            Log.Debug("Trying to serialize the MapSchematic...", Config.Debug);

            File.WriteAllText(path, Loader.Serializer.Serialize(map));

            Log.Debug("MapSchematic has been successfully saved to a file!", Config.Debug);

            Timing.CallDelayed(1f, () => Config.EnableFileSystemWatcher = prevValue);
        }

        /// <summary>
        /// Gets or sets the <see cref="MapSchematic"/> by it's name.
        /// </summary>
        /// <param name="mapName">The name of the map.</param>
        /// <returns><see cref="MapSchematic"/> if the file with the map was found, otherwise <see langword="null"/>.</returns>
        public static MapSchematic GetMapByName(string mapName)
        {
            string path = Path.Combine(MapEditorReborn.PluginDir, $"{mapName}.yml");

            if (!File.Exists(path))
                return null;

            return Loader.Deserializer.Deserialize<MapSchematic>(File.ReadAllText(path));
        }

        #endregion

        #region Spawning Objects Methods

        /// <summary>
        /// Spawns a door.
        /// </summary>
        /// <param name="door">The <see cref="DoorObject"/> which is used to spawn a door.</param>
        public static void SpawnDoor(DoorObject door)
        {
            Room room = GetRandomRoom(door.RoomType);
            GameObject gameObject = Object.Instantiate(door.DoorType.GetDoorObjectByType(), GetRelativePosition(door.Position, room), GetRelativeRotation(door.Rotation, room));
            gameObject.transform.localScale = door.Scale;

            gameObject.AddComponent<ObjectRotationComponent>().Init(door.Rotation);

            SpawnedObjects.Add(gameObject.AddComponent<DoorObjectComponent>().Init(door));
        }

        /// <summary>
        /// Spawns a workstation.
        /// </summary>
        /// <param name="workStation">The <see cref="WorkStationObject"/> to spawn.</param>
        public static void SpawnWorkStation(WorkStationObject workStation)
        {
            Room room = GetRandomRoom(workStation.RoomType);
            GameObject gameObject = Object.Instantiate(WorkstationObj, GetRelativePosition(workStation.Position, room), GetRelativeRotation(workStation.Rotation, room));
            gameObject.transform.localScale = workStation.Scale;

            gameObject.AddComponent<ObjectRotationComponent>().Init(workStation.Rotation);

            SpawnedObjects.Add(gameObject.AddComponent<WorkStationObjectComponent>().Init(workStation));
        }

        /// <summary>
        /// Spawns a ItemSpawnPoint.
        /// </summary>
        /// <param name="itemSpawnPoint">The <see cref="ItemSpawnPointObject"/> to spawn.</param>
        public static void SpawnItemSpawnPoint(ItemSpawnPointObject itemSpawnPoint)
        {
            Room room = GetRandomRoom(itemSpawnPoint.RoomType);
            GameObject gameObject = Object.Instantiate(ItemSpawnPointObj, GetRelativePosition(itemSpawnPoint.Position, room), GetRelativeRotation(itemSpawnPoint.Rotation, room));

            gameObject.AddComponent<ObjectRotationComponent>().Init(itemSpawnPoint.Rotation);

            SpawnedObjects.Add(gameObject.AddComponent<ItemSpawnPointComponent>().Init(itemSpawnPoint));
        }

        /// <summary>
        /// Spawns a PlayerSpawnPoint.
        /// </summary>
        /// <param name="playerSpawnPoint">The <see cref="PlayerSpawnPointObject"/> to spawn.</param>
        public static void SpawnPlayerSpawnPoint(PlayerSpawnPointObject playerSpawnPoint)
        {
            Room room = GetRandomRoom(playerSpawnPoint.RoomType);
            GameObject gameObject = Object.Instantiate(PlayerSpawnPointObj, GetRelativePosition(playerSpawnPoint.Position, room), Quaternion.identity);
            gameObject.tag = playerSpawnPoint.RoleType.ConvertToSpawnPointTag();

            SpawnedObjects.Add(gameObject.AddComponent<PlayerSpawnPointComponent>());
        }

        /// <summary>
        /// Spawns a RagdollSpawnPoint.
        /// </summary>
        /// <param name="ragdollSpawnPoint">The <see cref="RagdollSpawnPointObject"/> to spawn.</param>
        public static void SpawnRagdollSpawnPoint(RagdollSpawnPointObject ragdollSpawnPoint)
        {
            Room room = GetRandomRoom(ragdollSpawnPoint.RoomType);
            GameObject gameObject = Object.Instantiate(RagdollSpawnPointObj, GetRelativePosition(ragdollSpawnPoint.Position, room), GetRelativeRotation(ragdollSpawnPoint.Rotation, room));

            gameObject.AddComponent<ObjectRotationComponent>().Init(ragdollSpawnPoint.Rotation);

            SpawnedObjects.Add(gameObject.AddComponent<RagdollSpawnPointComponent>().Init(ragdollSpawnPoint));
        }

        /// <summary>
        /// Spawns a ShootingTarget.
        /// </summary>
        /// <param name="shootingTarget">The <see cref="ShootingTargetObject"/> to spawn.</param>
        public static void SpawnShootingTarget(ShootingTargetObject shootingTarget)
        {
            Room room = GetRandomRoom(shootingTarget.RoomType);
            GameObject gameObject = Object.Instantiate(shootingTarget.TargetType.GetShootingTargetObjectByType(), GetRelativePosition(shootingTarget.Position, room), GetRelativeRotation(shootingTarget.Rotation, room));
            gameObject.transform.localScale = shootingTarget.Scale;

            gameObject.AddComponent<ObjectRotationComponent>().Init(shootingTarget.Rotation);

            SpawnedObjects.Add(gameObject.AddComponent<ShootingTargetComponent>().Init(shootingTarget));
        }

        /// <summary>
        /// Spawns a LightController.
        /// </summary>
        /// <param name="lightController">The <see cref="LightControllerObject"/> to spawn.</param>
        public static void SpawnLightController(LightControllerObject lightController)
        {
            GameObject gameObject = Object.Instantiate(LightControllerObj);

            SpawnedObjects.Add(gameObject.AddComponent<LightControllerComponent>().Init(lightController));
        }

        /// <summary>
        /// Spawns a Teleporter.
        /// </summary>
        /// <param name="teleport">The <see cref="TeleportObject"/> to spawn.</param>
        public static void SpawnTeleport(TeleportObject teleport)
        {
            GameObject gameObject = Object.Instantiate(TeleporterObj);

            SpawnedObjects.Add(gameObject.AddComponent<TeleportControllerComponent>().Init(teleport));
        }

        /// <summary>
        /// Spawns a copy of selected object by a ToolGun.
        /// </summary>
        /// <param name="position">Position of spawned property object.</param>
        /// <param name="prefab">The <see cref="GameObject"/> from which the copy will be spawned.</param>
        public static void SpawnPropertyObject(Vector3 position, GameObject prefab)
        {
            GameObject gameObject = Object.Instantiate(prefab, position, prefab.transform.rotation);
            gameObject.name = gameObject.name.Replace("(Clone)(Clone)", "(Clone)");

            SpawnedObjects.Add(gameObject.GetComponent<MapEditorObject>());
            NetworkServer.Spawn(gameObject);
        }

        #endregion

        #region ToolGun Methods

        /// <summary>
        /// Spawns a general <see cref="MapEditorObject"/>.
        /// Used by the ToolGun.
        /// </summary>
        /// <param name="position">The postition of the spawned object.</param>
        /// <param name="mode">The current <see cref="ToolGunMode"/>.</param>
        public static void SpawnObject(Vector3 position, ToolGunMode mode)
        {
            GameObject gameObject = Object.Instantiate(mode.GetObjectByMode(), position, Quaternion.identity);
            gameObject.transform.rotation = GetRelativeRotation(Vector3.zero, Map.FindParentRoom(gameObject));

            switch (mode)
            {
                case ToolGunMode.LczDoor:
                case ToolGunMode.HczDoor:
                case ToolGunMode.EzDoor:
                    {
                        gameObject.AddComponent<DoorObjectComponent>().Init(new DoorObject());

                        break;
                    }

                case ToolGunMode.WorkStation:
                    {
                        gameObject.AddComponent<WorkStationObjectComponent>().Init(new WorkStationObject());
                        break;
                    }

                case ToolGunMode.ItemSpawnPoint:
                    {
                        gameObject.transform.position += Vector3.up * 0.1f;
                        gameObject.AddComponent<ItemSpawnPointComponent>().Init(new ItemSpawnPointObject());
                        break;
                    }

                case ToolGunMode.PlayerSpawnPoint:
                    {
                        gameObject.tag = "SP_173";
                        gameObject.transform.position += Vector3.up * 0.25f;
                        gameObject.AddComponent<PlayerSpawnPointComponent>().Init(new PlayerSpawnPointObject());
                        break;
                    }

                case ToolGunMode.RagdollSpawnPoint:
                    {
                        gameObject.transform.position += Vector3.up * 1.5f;
                        gameObject.AddComponent<RagdollSpawnPointComponent>().Init(new RagdollSpawnPointObject());
                        break;
                    }

                case ToolGunMode.SportShootingTarget:
                case ToolGunMode.DboyShootingTarget:
                case ToolGunMode.BinaryShootingTarget:
                    {
                        gameObject.AddComponent<ShootingTargetComponent>().Init(new ShootingTargetObject());
                        break;
                    }

                case ToolGunMode.LightController:
                    {
                        gameObject.transform.position += Vector3.up * 0.25f;
                        gameObject.AddComponent<LightControllerComponent>().Init(new LightControllerObject());
                        break;
                    }

                case ToolGunMode.Teleporter:
                    {
                        gameObject.transform.position += Vector3.up;
                        gameObject.AddComponent<TeleportControllerComponent>().Init(new TeleportObject());
                        break;
                    }
            }

            SpawnedObjects.Add(gameObject.GetComponent<MapEditorObject>());
        }

        /// <summary>
        /// Selects the <see cref="MapEditorObject"/>.
        /// </summary>
        /// <param name="player">The player that selects the object.</param>
        /// <param name="mapObject">The <see cref="MapEditorObject"/> to select.</param>
        public static void SelectObject(Player player, MapEditorObject mapObject)
        {
            if (mapObject != null && SpawnedObjects.Contains(mapObject))
            {
                player.ShowGameObjectHint(mapObject);

                if (!player.SessionVariables.ContainsKey(SelectedObjectSessionVarName))
                {
                    player.SessionVariables.Add(SelectedObjectSessionVarName, mapObject);
                }
                else
                {
                    player.SessionVariables[SelectedObjectSessionVarName] = mapObject;
                }
            }
            else if (player.SessionVariables.ContainsKey(SelectedObjectSessionVarName))
            {
                player.SessionVariables.Remove(SelectedObjectSessionVarName);
                player.ShowHint("Object have been unselected");
            }
        }

        /// <summary>
        /// Deletes the <see cref="MapEditorObject"/>.
        /// </summary>
        /// <param name="player">The player that deletes the object.</param>
        /// <param name="mapObject">The <see cref="MapEditorObject"/> to delete.</param>
        public static void DeleteObject(Player player, MapEditorObject mapObject)
        {
            if (player.TryGetSessionVariable(SelectedObjectSessionVarName, out MapEditorObject selectedObject) && selectedObject == mapObject)
            {
                player.SessionVariables.Remove(SelectedObjectSessionVarName);
                player.ShowHint(string.Empty, 0.1f);
            }

            SpawnedObjects.Remove(mapObject);
            mapObject.Destroy();
        }

        #endregion

        #region Getting Relative Stuff Methods

        /// <summary>
        /// Gets or sets a random <see cref="Room"/> from the <see cref="RoomType"/>.
        /// </summary>
        /// <param name="type">The <see cref="RoomType"/> from which the room should be choosen.</param>
        /// <returns>A random <see cref="Room"/> that has <see cref="Room.Type"/> of the argument.</returns>
        public static Room GetRandomRoom(RoomType type)
        {
            List<Room> validRooms = Map.Rooms.Where(x => x.Type == type).ToList();

            return validRooms[Random.Range(0, validRooms.Count)];
        }

        /// <summary>
        /// Gets or sets a position relative to the <see cref="Room"/>.
        /// </summary>
        /// <param name="position">The object position.</param>
        /// <param name="room">The <see cref="Room"/> whose <see cref="Transform"/> will be used.</param>
        /// <returns>Global position relative to the <see cref="Room"/>. If the <paramref name="type"/> is equal to <see cref="RoomType.Surface"/> the <paramref name="position"/> will be retured with no changes.</returns>
        public static Vector3 GetRelativePosition(Vector3 position, Room room)
        {
            if (room.Type == RoomType.Surface)
            {
                return position;
            }
            else
            {
                return room.transform.TransformPoint(position);
            }
        }

        /// <summary>
        /// Gets or sets a rotation relative to the <see cref="Room"/>.
        /// </summary>
        /// <param name="rotation">The object rotation.</param>
        /// <param name="room">The <see cref="Room"/> whose <see cref="Transform"/> will be used.</param>
        /// <returns>Global rotation relative to the <see cref="Room"/>. If the <paramref name="roomType"/> is equal to <see cref="RoomType.Surface"/> the <paramref name="rotation"/> will be retured with no changes.</returns>
        public static Quaternion GetRelativeRotation(Vector3 rotation, Room room)
        {
            if (rotation.x == -1f)
                rotation.x = Random.Range(0f, 360f);

            if (rotation.y == -1f)
                rotation.y = Random.Range(0f, 360f);

            if (rotation.z == -1f)
                rotation.z = Random.Range(0f, 360f);

            if (room.Type == RoomType.Surface)
            {
                return Quaternion.Euler(rotation);
            }
            else
            {
                return room.transform.rotation * Quaternion.Euler(rotation);
            }
        }

        #endregion

        #region Spawning Indicators

        public static void SpawnObjectIndicator(ItemSpawnPointComponent itemSpawnPoint, IndicatorObjectComponent indicatorObject = null)
        {
            Vector3 position = itemSpawnPoint.transform.position;
            Quaternion rotation = itemSpawnPoint.transform.rotation;

            if (indicatorObject != null)
            {
                indicatorObject.AttachedMapEditorObject.transform.position = position;
                indicatorObject.AttachedMapEditorObject.transform.rotation = rotation;
                return;
            }

            ItemType parsedItem;

            if (CustomItem.TryGet(itemSpawnPoint.Base.Item, out CustomItem custom))
            {
                parsedItem = custom.Type;
            }
            else
            {
                parsedItem = (ItemType)Enum.Parse(typeof(ItemType), itemSpawnPoint.Base.Item, true);
            }

            Pickup pickup = new Item(parsedItem).Spawn(position + (Vector3.up * 0.1f), rotation);
            pickup.Locked = true;

            GameObject pickupGameObject = pickup.Base.gameObject;
            NetworkServer.UnSpawn(pickupGameObject);

            pickupGameObject.GetComponent<Rigidbody>().isKinematic = true;

            if (parsedItem.IsWeapon())
                pickupGameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            pickupGameObject.AddComponent<ItemSpiningComponent>();

            IndicatorObjectComponent objectIndicator = pickupGameObject.AddComponent<IndicatorObjectComponent>();
            objectIndicator.Init(itemSpawnPoint);

            SpawnedObjects.Add(objectIndicator);
            NetworkServer.Spawn(pickupGameObject);
        }

        public static void SpawnObjectIndicator(PlayerSpawnPointComponent playerSpawnPoint, IndicatorObjectComponent indicatorObject = null)
        {
            Vector3 position = playerSpawnPoint.transform.position;

            if (indicatorObject != null)
            {
                ReferenceHub dummyIndicator = indicatorObject.GetComponent<ReferenceHub>();

                try
                {
                    dummyIndicator.transform.position = position;
                    dummyIndicator.playerMovementSync.OverridePosition(position, 0f);
                }
                catch
                {
                    return;
                }

                return;
            }

            GameObject dummyObject = Object.Instantiate(LiteNetLib4MirrorNetworkManager.singleton.playerPrefab);
            dummyObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            dummyObject.transform.position = position;

            RoleType roleType = playerSpawnPoint.tag.ConvertToRoleType();

            QueryProcessor processor = dummyObject.GetComponent<QueryProcessor>();

            processor.NetworkPlayerId = QueryProcessor._idIterator++;
            processor._ipAddress = "127.0.0.WAN";

            CharacterClassManager ccm = dummyObject.GetComponent<CharacterClassManager>();
            ccm.CurClass = playerSpawnPoint.tag.ConvertToRoleType();
            ccm.GodMode = true;

            string dummyNickname = roleType.ToString();

            switch (roleType)
            {
                case RoleType.NtfPrivate:
                    dummyNickname = "MTF";
                    break;

                case RoleType.Scp93953:
                    dummyNickname = "SCP939";
                    break;
            }

            NicknameSync nicknameSync = dummyObject.GetComponent<NicknameSync>();
            nicknameSync.Network_myNickSync = "PLAYER SPAWNPOINT";
            nicknameSync.CustomPlayerInfo = $"{dummyNickname}\nSPAWN POINT";
            nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Nickname;
            nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;

            IndicatorObjectComponent objectIndicator = dummyObject.AddComponent<IndicatorObjectComponent>();
            objectIndicator.Init(playerSpawnPoint);

            SpawnedObjects.Add(objectIndicator);
            NetworkServer.Spawn(dummyObject);
            /*
            // PlayerManager.players.Add(dummyObject);
            // Indicators.Add(objectIndicator, playerSpawnPoint);
            */

            ReferenceHub rh = dummyObject.GetComponent<ReferenceHub>();
            Timing.CallDelayed(0.5f, () =>
            {
                // dummyObject.AddComponent<DummySpiningComponent>().Hub = rh;
                rh.playerMovementSync.OverridePosition(position, 0f);
            });
        }

        public static void SpawnObjectIndicator(RagdollSpawnPointComponent ragdollSpawnPoint, IndicatorObjectComponent indicatorObject = null)
        {
            Vector3 position = ragdollSpawnPoint.transform.position;

            if (indicatorObject != null)
            {
                ReferenceHub dummyIndicator = indicatorObject.GetComponent<ReferenceHub>();

                try
                {
                    dummyIndicator.transform.position = position;
                    dummyIndicator.playerMovementSync.OverridePosition(position, 0f);
                }
                catch
                {
                    return;
                }

                return;
            }

            GameObject dummyObject = Object.Instantiate(LiteNetLib4MirrorNetworkManager.singleton.playerPrefab);
            dummyObject.transform.localScale = new Vector3(-0.2f, -0.2f, -0.2f);
            dummyObject.transform.position = position;

            RoleType roleType = ragdollSpawnPoint.Base.RoleType;

            QueryProcessor processor = dummyObject.GetComponent<QueryProcessor>();
            processor.NetworkPlayerId = QueryProcessor._idIterator++;
            processor._ipAddress = "127.0.0.WAN";

            CharacterClassManager ccm = dummyObject.GetComponent<CharacterClassManager>();
            ccm.CurClass = roleType;
            ccm.GodMode = true;

            string dummyNickname = roleType.ToString();

            switch (roleType)
            {
                case RoleType.NtfPrivate:
                    dummyNickname = "MTF";
                    break;

                case RoleType.Scp93953:
                    dummyNickname = "SCP939";
                    break;
            }

            NicknameSync nicknameSync = dummyObject.GetComponent<NicknameSync>();
            nicknameSync.Network_myNickSync = "RAGDOLL SPAWNPOINT";
            nicknameSync.CustomPlayerInfo = $"{dummyNickname} RAGDOLL\nSPAWN POINT";
            nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Nickname;
            nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;

            IndicatorObjectComponent objectIndicator = dummyObject.AddComponent<IndicatorObjectComponent>();
            objectIndicator.Init(ragdollSpawnPoint);

            SpawnedObjects.Add(objectIndicator);
            NetworkServer.Spawn(dummyObject);
            /*
            // PlayerManager.players.Add(dummyObject);
            // Indicators.Add(objectIndicator, ragdollSpawnPoint);
            */

            ReferenceHub rh = dummyObject.GetComponent<ReferenceHub>();
            Timing.CallDelayed(0.5f, () =>
            {
                // dummyObject.AddComponent<DummySpiningComponent>().Hub = rh;
                rh.playerMovementSync.OverridePosition(position, 0f);
            });
        }

        #endregion
    }
}