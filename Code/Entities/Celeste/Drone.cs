﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class Drone : Actor
    {
        public Sprite droneSprite;

        public bool dead;

        public bool enabled;

        public bool released;

        public static bool canControl;

        public bool faceLeft;

        public Vector2 Speed;

        public float DestroyTimer;

        public bool Flashing;

        public float noGravityTimer;

        public Player player;

        public FakePlayer FakePlayer;

        private Collision onCollideH;

        private Collision onCollideV;

        public static Holdable Hold;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private string startRoom;

        public bool lookUp;

        public bool Transitioning;

        public float previousSpriteRate;

        public bool MapOpen;

        public int previousPlayerState = -1;

        public float BeamDelay;

        public VertexLight Light;

        public Vector2? CurrentSpawn;

        public static bool ShouldGetTalkerStatus = true;

        public static bool TalkersStatus;

        public bool Teleport;

        public Vector2 TeleportSpawn;

        public string previousRoom;

        public string currentRoom;

        public Vector2 cameraPosition;

        public bool canDestroy;

        public bool startedAsDrone;

        public int CurrentMissiles;

        public int CurrentSuperMissiles;

        private Coroutine DestroyRoutine = new();

        private static FieldInfo HoldableCannotHoldTimer = typeof(Holdable).GetField("cannotHoldTimer", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo PlayerOnGround = typeof(Player).GetField("onGround", BindingFlags.Instance | BindingFlags.NonPublic);

        public Drone(Vector2 position, Player player) : base(position)
        {
            Tag = Tags.Persistent;
            this.player = player;
            Collider = new Hitbox(8f, 6f, -4f, -6f);
            Add(droneSprite = new Sprite(GFX.Game, "characters/Xaphan/remote_drone/"));
            droneSprite.Add("egg", "egg", 1, 0);
            droneSprite.Add("hatch", "egg", 0.08f, 1, 2, 3, 4, 5, 6, 7);
            droneSprite.AddLoop("idle", "idle", 0.1f);
            droneSprite.Add("lookUp", "lookUp", 0.06f);
            droneSprite.AddLoop("lookUpLoop", "lookUp", 1, 4);
            droneSprite.AddLoop("walk", "walk", 0.06f);
            droneSprite.AddLoop("walkUp", "walk_up", 0.06f);
            droneSprite.Add("jump", "jump", 0.1f, 0, 1);
            droneSprite.Add("fall", "jump", 0.1f, 2, 3);
            droneSprite.Add("jumpUp", "jump_up", 0.1f, 0, 1);
            droneSprite.Add("fallUp", "jump_up", 0.1f, 2, 3);
            droneSprite.CenterOrigin();
            droneSprite.Justify = new Vector2(0.5f, 1f);
            droneSprite.Play("egg");
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(6f, 4f, -3f, -6f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.SpeedGetter = (() => Speed);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Add(Light = new VertexLight(new Vector2(0f, -5f), Color.White, 1f, 32, 64));
            Depth = 0;
        }

        public static void Load()
        {
            On.Celeste.TalkComponent.Update += OnTalkComponentUpdate;
            On.Celeste.Holdable.Update += OnHoldableUpdate;
            On.Celeste.Player.Die += OnCelestePlayerDie;
            On.Celeste.Player.Jump += OnPlayerjump;
            On.Celeste.Player.Throw += OnPlayerThrow;
            On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (XaphanModule.startAsDrone)
            {
                Player player = self.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    player.Position = (self.Session.Level == XaphanModule.droneStartRoom && self.Session.RespawnPoint.GetValueOrDefault() == XaphanModule.droneCurrentSpawn) ? XaphanModule.fakePlayerPosition : self.Session.RespawnPoint.GetValueOrDefault();
                    self.Camera.Position = player.CameraTarget;
                    self.Add(new Drone(player.Position, player)
                    {
                        canDestroy = true,
                        startRoom = XaphanModule.droneStartRoom,
                        startedAsDrone = true
                    });
                    XaphanModule.droneStartRoom = null;
                    player.Visible = false;
                    DynData<Player> playerData = new(player);
                    Hitbox normalPlayerHitbox = playerData.Get<Hitbox>("normalHitbox");
                    normalPlayerHitbox.Height = 6f;
                    normalPlayerHitbox.Width = 6f;
                    normalPlayerHitbox.Left = -3f;
                    normalPlayerHitbox.Top = -6f;
                    Hitbox normalPlayerHurtbox = playerData.Get<Hitbox>("normalHurtbox");
                    normalPlayerHurtbox.Height = 4f;
                    normalPlayerHurtbox.Width = 6f;
                    normalPlayerHurtbox.Left = -3f;
                    normalPlayerHurtbox.Top = -6f;
                    Hitbox duckPlayerHitbox = playerData.Get<Hitbox>("duckHitbox");
                    duckPlayerHitbox.Height = 6f;
                    duckPlayerHitbox.Width = 6f;
                    duckPlayerHitbox.Left = -3f;
                    duckPlayerHitbox.Top = -6f;
                    Hitbox duckPlayerHurtbox = playerData.Get<Hitbox>("duckHurtbox");
                    duckPlayerHurtbox.Height = 4f;
                    duckPlayerHurtbox.Width = 6f;
                    duckPlayerHurtbox.Left = -3f;
                    duckPlayerHurtbox.Top = -6f;
                }
            }
        }

        private static void OnPlayerThrow(On.Celeste.Player.orig_Throw orig, Player self)
        {
            if (self.Holding != Hold)
            {
                orig(self);
            }
            else
            {
                if ((bool)PlayerOnGround.GetValue(self))
                {
                    orig(self);
                }
            }
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            orig(self, player);
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            Drone drone = self.SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (onSolid && XaphanModule.PlayerIsControllingRemoteDrone() && drone != null)
            {
                XaphanModule.ModSession.CurrentDroneMissile = drone.CurrentMissiles;
                XaphanModule.ModSession.CurrentDroneSuperMissile = drone.CurrentSuperMissiles;
                drone.CurrentSpawn = self.Position + self.Target;
            }
        }

        public static void Unload()
        {
            On.Celeste.TalkComponent.Update -= OnTalkComponentUpdate;
            On.Celeste.Holdable.Update -= OnHoldableUpdate;
            On.Celeste.Player.Die -= OnCelestePlayerDie;
            On.Celeste.Player.Jump -= OnPlayerjump;
            On.Celeste.Player.Throw -= OnPlayerThrow;
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void OnHoldableUpdate(On.Celeste.Holdable.orig_Update orig, Holdable self)
        {
            if (!XaphanModule.PlayerIsControllingRemoteDrone())
            {
                orig(self);
            }
            else
            {
                HoldableCannotHoldTimer.SetValue(self, 1f);
            }
        }

        private static void OnTalkComponentUpdate(On.Celeste.TalkComponent.orig_Update orig, TalkComponent self)
        {
            if (!XaphanModule.PlayerIsControllingRemoteDrone() && !ShouldGetTalkerStatus)
            {
                self.Enabled = TalkersStatus;
                ShouldGetTalkerStatus = true;
            }
            if (!XaphanModule.PlayerIsControllingRemoteDrone())
            {
                orig(self);
            }
            else
            {
                if (ShouldGetTalkerStatus)
                {
                    TalkersStatus = self.Enabled;
                    ShouldGetTalkerStatus = false;
                }
                self.Enabled = false;
            }
        }

        private static PlayerDeadBody OnCelestePlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            Drone drone = self.SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (drone != null)
            {
                if (Hold.IsHeld)
                {
                    drone.RemoveSelf();
                    return orig(self, direction, evenIfInvincible, registerDeathInStats);
                }
                else if (!drone.enabled)
                {
                    drone.player.Hair.Color = Calc.HexToColor("D68662");
                    if (self != drone.FakePlayer)
                    {
                        if (!drone.dead)
                        {
                            drone.ForceDestroy();
                        }
                    }
                    else
                    {
                        if (!drone.dead)
                        {
                            drone.ForceDestroy(true, true);
                        }
                        return orig(self, direction, evenIfInvincible, registerDeathInStats);
                    }
                }
                else if (!drone.dead)
                {
                    drone.player.Hair.Color = Calc.HexToColor("D68662");
                    if (self == drone.FakePlayer)
                    {
                        if (!drone.dead)
                        {
                            drone.ForceDestroy(true, true);
                        }
                        return orig(self, direction, evenIfInvincible, registerDeathInStats);
                    }
                    drone.ForceDestroy();
                }
                return null;
            }
            else
            {
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }
        }

        private static void OnPlayerjump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {
            Drone drone = self.SceneAs<Level>().Tracker.GetEntity<Drone>();
            if (drone != null && drone.released)
            {
                orig(self, false, false);
            }
            else
            {
                orig(self, particles, playSfx);
            }
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data, 3, 3) && !SaveData.Instance.Assists.Invincible)
            {
                ForceDestroy();
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!XaphanModule.ModSettings.UseBagItemSlot.Check && !XaphanModule.startAsDrone)
            {
                RemoveSelf();
            }
            else
            {
                if (startedAsDrone)
                {
                    FakePlayer = new FakePlayer(XaphanModule.fakePlayerPosition, SceneAs<Level>().Session.Inventory.Backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack, true);
                    FakePlayer.Facing = XaphanModule.fakePlayerFacing;
                    FakePlayer.StateMachine.State = 11;
                    FakePlayer.DummyAutoAnimate = false;
                    FakePlayer.DummyGravity = false;
                    FakePlayer.Depth = 100;
                    Scene.Add(FakePlayer);
                    CurrentSpawn = XaphanModule.droneCurrentSpawn;
                    GiveAmmo();
                }
                else
                {
                    SceneAs<Level>().PauseLock = true;
                }
            }
            XaphanModule.startAsDrone = false;
            XaphanModule.droneCurrentSpawn = null;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (FakePlayer != null)
            {
                FakePlayer.RemoveSelf();
            }
        }

        private void ForceDestroy(bool normalRespawn = false, bool silence = false)
        {
            if (!DestroyRoutine.Active)
            {
                Add(DestroyRoutine = new Coroutine(Destroy(normalRespawn, silence)));
            }
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().PauseLock = false;
            AllowPushing = false;
        }

        private void OnRelease(Vector2 force)
        {
            if (player != null)
            {
                GiveAmmo();
                DynData<Player> playerData = new(player);
                Hitbox normalPlayerHitbox = playerData.Get<Hitbox>("normalHitbox");
                normalPlayerHitbox.Height = 6f;
                normalPlayerHitbox.Width = 6f;
                normalPlayerHitbox.Left = -3f;
                normalPlayerHitbox.Top = -6f;
                Hitbox normalPlayerHurtbox = playerData.Get<Hitbox>("normalHurtbox");
                normalPlayerHurtbox.Height = 4f;
                normalPlayerHurtbox.Width = 6f;
                normalPlayerHurtbox.Left = -3f;
                normalPlayerHurtbox.Top = -6f;
                Hitbox duckPlayerHitbox = playerData.Get<Hitbox>("duckHitbox");
                duckPlayerHitbox.Height = 6f;
                duckPlayerHitbox.Width = 6f;
                duckPlayerHitbox.Left = -3f;
                duckPlayerHitbox.Top = -6f;
                Hitbox duckPlayerHurtbox = playerData.Get<Hitbox>("duckHurtbox");
                duckPlayerHurtbox.Height = 4f;
                duckPlayerHurtbox.Width = 6f;
                duckPlayerHurtbox.Left = -3f;
                duckPlayerHurtbox.Top = -6f;
                CurrentSpawn = SceneAs<Level>().Session.RespawnPoint;
                cameraPosition = new Vector2(SceneAs<Level>().Camera.Position.X - SceneAs<Level>().Bounds.Left, SceneAs<Level>().Camera.Position.Y - SceneAs<Level>().Bounds.Top);
                released = true;
                startRoom = SceneAs<Level>().Session.Level;
                Scene.Add(FakePlayer = new FakePlayer(player.Position, SceneAs<Level>().Session.Inventory.Backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack));
                FakePlayer.Facing = player.Facing;
                FakePlayer.StateMachine.State = 11;
                FakePlayer.DummyAutoAnimate = false;
                FakePlayer.Sprite.Play("throw");
                FakePlayer.Depth = 100;
                XaphanModule.fakePlayerFacing = FakePlayer.Facing;
                XaphanModule.fakePlayerPosition = FakePlayer.Position;
                player.Visible = false;
                if (force.X != 0f && force.Y == 0f)
                {
                    force.Y = -0.4f;
                }
                Speed = force * 200f;
                if (Speed != Vector2.Zero)
                {
                    noGravityTimer = 0.1f;
                }
                Add(new Coroutine(DestroyDelay()));
            }
            AllowPushing = true;
        }

        private IEnumerator DestroyDelay()
        {
            float delay = 0.5f;
            while (delay > 0)
            {
                delay -= Engine.DeltaTime;
                yield return null;
            }
            canDestroy = true;
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= enabled ? 1f : 0.5f;
                    Speed.Y = enabled ? -320f : -160f;
                    noGravityTimer = enabled ? 0 : 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = enabled ? 330f : 220f;
                    Speed.Y = enabled ? -160f : -80f;
                    noGravityTimer = enabled ? 0 : 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = enabled ? -330f : -220f;
                    Speed.Y = enabled ? -160f : -80f;
                    noGravityTimer = enabled ? 0 : 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (!enabled)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            }
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.2f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Direction == new Vector2(0, 1f))
            {
                if (!enabled && Speed.Y > 0f)
                {
                    Audio.Play("event:/game/xaphan/drone_spawn");
                }
                if (Speed.Y > 160f)
                {
                    ImpactParticles(data.Direction);
                }
                Speed.Y = 0f;
                if (!enabled)
                {
                    enabled = true;
                    canControl = true;
                    Speed = Vector2.Zero;
                    Add(new Coroutine(HatchingRoutine()));
                }
            }
        }

        private IEnumerator HatchingRoutine()
        {
            droneSprite.Play("hatch");
            while (droneSprite.CurrentAnimationID == "hatch")
            {
                player.StateMachine.State = 11;
                yield return null;
                player.Position.Y = (float)Math.Floor(player.Position.Y);
            }

            player.StateMachine.State = 0;
        }

        private void startLookUp(string s)
        {
            lookUp = true;
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position = default(Vector2);
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            SceneAs<Level>().Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public void TeleportPlayerToDrone()
        {
            Teleport = true;
            SceneAs<Level>().Session.RespawnPoint = SceneAs<Level>().GetSpawnPoint(TeleportSpawn);
            Audio.Play("event:/char/badeline/disappear");
            SceneAs<Level>().Displacement.AddBurst(FakePlayer.Center, 0.5f, 8f, 32f, 0.5f);
            SceneAs<Level>().Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
            player.Visible = true;
            DynData<Player> playerData = new(player);
            Hitbox normalPlayerHitbox = playerData.Get<Hitbox>("normalHitbox");
            normalPlayerHitbox.Height = 11f;
            normalPlayerHitbox.Width = 8f;
            normalPlayerHitbox.Left = -4f;
            normalPlayerHitbox.Top = -11f;
            Hitbox normalPlayerHurtbox = playerData.Get<Hitbox>("normalHurtbox");
            normalPlayerHurtbox.Height = 9f;
            normalPlayerHurtbox.Width = 8f;
            normalPlayerHurtbox.Left = -4f;
            normalPlayerHurtbox.Top = -11f;
            Hitbox duckPlayerHitbox = playerData.Get<Hitbox>("duckHitbox");
            duckPlayerHitbox.Height = 6f;
            duckPlayerHitbox.Width = 8f;
            duckPlayerHitbox.Left = -4f;
            duckPlayerHitbox.Top = -6f;
            Hitbox duckPlayerHurtbox = playerData.Get<Hitbox>("duckHurtbox");
            duckPlayerHurtbox.Height = 4f;
            duckPlayerHurtbox.Width = 8f;
            duckPlayerHurtbox.Left = -4f;
            duckPlayerHurtbox.Top = -6f;
            SceneAs<Level>().CanRetry = true;
            SceneAs<Level>().SaveQuitDisabled = false;
            RemoveSelf();
        }

        public void GiveAmmo()
        {
            string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            if (XaphanModule.ModSession.CurrentDroneMissile == 0)
            {
                int missileCount = 10;
                foreach (string missileUpgrade in (XaphanModule.PlayerHasGolden || XaphanModule.ModSettings.SpeedrunMode) ? XaphanModule.ModSaveData.SpeedrunModeDroneMissilesUpgrades : XaphanModule.ModSaveData.DroneMissilesUpgrades)
                {
                    if (missileUpgrade.Contains(Prefix))
                    {
                        missileCount += 2;
                    }
                }
                CurrentMissiles = missileCount;
                XaphanModule.ModSession.CurrentDroneMissile = missileCount;
            }
            else
            {
                CurrentMissiles = XaphanModule.ModSession.CurrentDroneMissile;
            }
            if (XaphanModule.ModSession.CurrentDroneSuperMissile == 0)
            {
                int superMissileCount = 5;
                foreach (string superMissileUpgrade in (XaphanModule.PlayerHasGolden || XaphanModule.ModSettings.SpeedrunMode) ? XaphanModule.ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades : XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
                {
                    if (superMissileUpgrade.Contains(Prefix))
                    {
                        superMissileCount++;
                    }
                }
                CurrentSuperMissiles = superMissileCount;
                XaphanModule.ModSession.CurrentDroneSuperMissile = superMissileCount;
            }
            else
            {
                CurrentSuperMissiles = XaphanModule.ModSession.CurrentDroneSuperMissile;
            }
        }

        public override void Update()
        {
            base.Update();
            UpgradesDisplay display = SceneAs<Level>().Tracker.GetEntity<UpgradesDisplay>();
            if (display != null)
            {
                display.CurrentMissiles = CurrentMissiles;
                display.CurrentSuperMissiles = CurrentSuperMissiles;
            }
            if (SceneAs<Level>().Transitioning)
            {
                if (XaphanModule.ModSession.CurrentDroneMissile != CurrentMissiles)
                {
                    XaphanModule.ModSession.CurrentDroneMissile = CurrentMissiles;
                }
                if (XaphanModule.ModSession.CurrentDroneSuperMissile != CurrentSuperMissiles)
                {
                    XaphanModule.ModSession.CurrentDroneSuperMissile = CurrentSuperMissiles;
                }
            }
            if (!dead && !Teleport)
            {
                SceneAs<Level>().CanRetry = false;
                SceneAs<Level>().SaveQuitDisabled = true;
            }
            else
            {
                SceneAs<Level>().CanRetry = true;
                SceneAs<Level>().SaveQuitDisabled = false;
            }
            currentRoom = SceneAs<Level>().Session.Level;
            if (currentRoom != previousRoom)
            {
                TeleportSpawn = SceneAs<Level>().Session.LevelData.Spawns.ClosestTo(player.Position);
                previousRoom = currentRoom;
            }
            if (DroneTeleport.Active(SceneAs<Level>()) && XaphanModule.ModSettings.UseBagItemSlot.Pressed && !Hold.IsHeld && enabled && player.StateMachine.State != 11 && !Teleport)
            {
                List<Entity> droneGates = SceneAs<Level>().Tracker.GetEntities<DroneGate>().ToList();
                droneGates.ForEach(entity => entity.Collidable = true);
                if (!CollideCheck<DroneGate>(player.Position) && !CollideCheck<DroneGate>(player.Position - Vector2.UnitY * 4))
                {
                    bool CollideUp = false;
                    bool CollideLeft = false;
                    bool CollideRight = false;
                    if (CollideCheck<Solid>(player.Position - Vector2.UnitY * 4))
                    {
                        CollideUp = true;
                    }
                    if (CollideCheck<Solid>(player.Position - Vector2.UnitX))
                    {
                        CollideLeft = true;
                        if (CollideCheck<Solid>(player.Position + Vector2.UnitX))
                        {
                            CollideRight = true;
                            Audio.Play("event:/game/general/assist_nonsolid_in");
                        }
                    }
                    else if (CollideCheck<Solid>(player.Position + Vector2.UnitX))
                    {
                        CollideRight = true;
                        if (CollideCheck<Solid>(player.Position - Vector2.UnitX))
                        {
                            CollideLeft = true;
                            Audio.Play("event:/game/general/assist_nonsolid_in");
                        }
                    }
                    else if (!CollideUp)
                    {
                        TeleportPlayerToDrone();
                    }
                    else
                    {
                        Audio.Play("event:/game/general/assist_nonsolid_in");
                    }
                    if (CollideLeft && !CollideRight)
                    {
                        if (!CollideCheck<Solid>(player.TopRight - Vector2.UnitY * 4))
                        {
                            player.MoveH(1);
                            TeleportPlayerToDrone();
                        }
                        else
                        {
                            Audio.Play("event:/game/general/assist_nonsolid_in");
                        }
                    }
                    else if (CollideRight && !CollideLeft)
                    {
                        if (!CollideCheck<Solid>(player.TopLeft - Vector2.UnitY * 4))
                        {
                            player.MoveH(-1);
                            TeleportPlayerToDrone();
                        }
                        else
                        {
                            Audio.Play("event:/game/general/assist_nonsolid_in");
                        }
                    }
                    if (!CollideUp)
                    {
                        TeleportPlayerToDrone();
                    }
                }
                else
                {
                    Audio.Play("event:/game/general/assist_nonsolid_in");
                }
                droneGates.ForEach(entity => entity.Collidable = false);
            }
            /*if (CurrentSpawn != null && !Teleport)
            {
                SceneAs<Level>().Session.RespawnPoint = CurrentSpawn;
            }*/
            if (!enabled)
            {
                Light.Visible = false;
                Hold.CheckAgainstColliders();
            }
            else
            {
                Light.Visible = true;
                AddTag(Tags.TransitionUpdate);
                if (Input.Dash.Check && BeamDelay <= 0 && !SceneAs<Level>().Transitioning && droneSprite.CurrentAnimationID != "egg" && droneSprite.CurrentAnimationID != "hatch" && droneSprite.CurrentAnimationID != "egg" && !dead && player.StateMachine.State == 0)
                {
                    Add(new Coroutine(Shoot(SceneAs<Level>())));
                }
            }
            foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
            {
                if ((liquid.liquidType == "water" || liquid.liquidType.Contains("acid")) && !dead && Hold.IsHeld)
                {
                    if (liquid.CollideCheck<Drone>())
                    {
                        SceneAs<Level>().Displacement.AddBurst(Position, 0.3f, 0f, 80f);
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                        Audio.Play("event:/game/xaphan/drone_destroy", Position);
                        DroneDebris.Burst(Position, Calc.HexToColor("DEAC75"), 12);
                        RemoveSelf();
                    }
                }
            }
            if (FakePlayer != null && !FakePlayer.Dead)
            {
                foreach (PlayerPlatform playerPlatform in SceneAs<Level>().Tracker.GetEntities<PlayerPlatform>())
                {
                    if (CollideCheck(playerPlatform, FakePlayer.Position + Vector2.UnitY))
                    {
                        FakePlayer.DummyGravity = false;
                        break;
                    }
                }
                if (SceneAs<Level>().Session.Level != startRoom)
                {
                    FakePlayer.DummyGravity = false;
                }
                if (FakePlayer.Sprite.CurrentAnimationID == "idle")
                {
                    FakePlayer.Sprite.Rate = 2f;
                    FakePlayer.Sprite.Play("sleep");
                }
                if (FakePlayer.Top > SceneAs<Level>().Bounds.Bottom && !SceneAs<Level>().Transitioning && currentRoom == startRoom)
                {
                    FakePlayer.RemoveSelf();
                }
            }
            if (player != null)
            {
                if (dead)
                {
                    return;
                }
                if (player.Facing == Facings.Right)
                {
                    droneSprite.FlipX = false;
                }
                else
                {
                    droneSprite.FlipX = true;
                }
                if (swatTimer > 0f)
                {
                    swatTimer -= Engine.DeltaTime;
                }
                if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
                {
                    hitSeeker = null;
                }
                if (!Hold.IsHeld && player.StateMachine.State != Player.StBoost && player.StateMachine.State != Player.StRedDash)
                {
                    if (!enabled)
                    {
                        float num = 800f;
                        if (Math.Abs(Speed.Y) <= 30f)
                        {
                            num *= 0.5f;
                        }
                        float num2 = 350f;
                        if (Speed.Y < 0f)
                        {
                            num2 *= 0.5f;
                        }
                        Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                        if (noGravityTimer > 0f)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                        }
                        MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                        MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                    }
                }
                if ((player != null || player.StateMachine.State == Player.StBoost || player.StateMachine.State == Player.StRedDash) && player.StateMachine.State != Player.StDummy)
                {
                    if ((Input.Grab.Pressed || (DestroyTimer > 0 && Input.Grab.Check)) && !XaphanModule.ModSettings.SelectItem.Check && !Hold.IsHeld && canDestroy && player.OnSafeGround && player.Speed == Vector2.Zero)
                    {
                        DestroyTimer += Engine.DeltaTime;
                        if (DestroyTimer >= 0.5f)
                        {
                            Add(new Coroutine(Destroy(forced: true)));
                        }
                    }
                    else
                    {
                        DestroyTimer = 0f;
                        Flashing = false;
                    }
                    if (DestroyTimer > 0)
                    {
                        if (Scene.OnInterval(0.06f))
                        {
                            Flashing = !Flashing;
                        }
                    }
                    if (Flashing)
                    {
                        droneSprite.Color = Color.Red;
                    }
                    else
                    {
                        if (HoverJet.Floating)
                        {
                            if (HoverJet.floatTimer <= 0.65f && player.Scene.OnRawInterval(0.06f))
                            {
                                if (droneSprite.Color == Color.Red)
                                {
                                    droneSprite.Color = Color.White;
                                }
                                else if (droneSprite.Color == Color.White)
                                {
                                    droneSprite.Color = Color.Red;
                                }
                            }
                        }
                        else
                        {
                            droneSprite.Color = Color.White;
                        }
                    }
                }
                if (enabled)
                {
                    Position = player.Position;
                }
                if (player != null && !enabled && !Hold.IsHeld && player.StateMachine.State != Player.StBoost && player.StateMachine.State != Player.StRedDash)
                {
                    player.Position = Position;
                }
                if (!Hold.IsHeld && enabled && canControl && droneSprite.CurrentAnimationID != "hatch" && droneSprite.CurrentAnimationID != "egg")
                {
                    if (SceneAs<Level>().Transitioning && !Transitioning)
                    {
                        previousSpriteRate = droneSprite.Rate;
                        droneSprite.Rate = 0;
                        Transitioning = true;
                    }
                    else if (previousSpriteRate != 0)
                    {
                        droneSprite.Rate = previousSpriteRate;
                        previousSpriteRate = 0;
                        Transitioning = false;
                    }
                    MapScreen mapScreen = SceneAs<Level>().Tracker.GetEntity<MapScreen>();
                    if (mapScreen == null)
                    {
                        if ((bool)PlayerOnGround.GetValue(player))
                        {
                            if (Input.MoveX == 0 && Input.MoveY == -1)
                            {
                                if (!lookUp)
                                {
                                    if (droneSprite.LastAnimationID == "walkUp")
                                    {
                                        lookUp = true;
                                    }
                                    else
                                    {
                                        droneSprite.Play("lookUp");
                                        droneSprite.OnLastFrame += startLookUp;
                                    }
                                }
                                else
                                {
                                    droneSprite.Play("lookUpLoop");
                                }
                            }
                            else
                            {
                                lookUp = false;
                                if ((Input.Aim.Value == new Vector2(1, -1) || Input.Aim.Value == new Vector2(-1, -1)))
                                {
                                    droneSprite.Play("walkUp");
                                }
                                else if (Input.MoveX != 0 && Input.MoveY != 1)
                                {
                                    droneSprite.Play("walk");
                                }
                                else
                                {
                                    droneSprite.Play("idle");
                                }
                            }
                        }
                        else
                        {
                            if (HoverJet.Active(SceneAs<Level>()) && HoverJet.Floating)
                            {
                                droneSprite.Play("jump");
                                droneSprite.SetAnimationFrame(0);

                            }
                            else
                            {
                                if (player.Speed.Y < 0 && Input.Aim.Value.SafeNormalize().Y >= 0)
                                {
                                    droneSprite.Play("jump");
                                }
                                else if (player.Speed.Y < 0 && Input.Aim.Value.Y == -1)
                                {
                                    droneSprite.Play("jumpUp");
                                }
                                else if (player.Speed.Y > 0 && Input.Aim.Value.SafeNormalize().Y >= 0)
                                {
                                    droneSprite.Play("fall");
                                }
                                else if (player.Speed.Y > 0 && Input.Aim.Value.Y == -1)
                                {
                                    droneSprite.Play("fallUp");
                                }
                            }
                        }
                    }
                    else
                    {
                        droneSprite.Play("idle");
                    }
                }
            }
            else
            {
                RemoveSelf();
            }
        }

        private IEnumerator Shoot(Level level)
        {
            UpgradesDisplay ammoDisplay = player.SceneAs<Level>().Tracker.GetEntity<UpgradesDisplay>();
            if (ammoDisplay != null)
            {
                if (!ammoDisplay.MissileSelected && !ammoDisplay.SuperMissileSelected)
                {
                    string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                    string beamSound = "event:/game/xaphan/drone" + (IceBeam.Active(level) ? "_ice" : (WaveBeam.Active(level) ? "_wave" : "")) + "_fire";
                    string beamType = "Power" + (WaveBeam.Active(level) ? "Wave" : "") + (IceBeam.Active(level) ? "Ice" : "");
                    level.Add(new Beam(player, beamType, beamSound, Position, WaveBeam.Active(level) ? 4 : 0));
                    int droneFireRateUpgradesCount = 0;
                    foreach (string fireRateModuleUpgrade in (XaphanModule.PlayerHasGolden || XaphanModule.ModSettings.SpeedrunMode) ? XaphanModule.ModSaveData.SpeedrunModeDroneFireRateUpgrades : XaphanModule.ModSaveData.DroneFireRateUpgrades)
                    {
                        if (fireRateModuleUpgrade.Contains(Prefix))
                        {
                            droneFireRateUpgradesCount++;
                        }
                    }
                    BeamDelay = 0.5f - droneFireRateUpgradesCount * 0.075f;
                }
                else if (ammoDisplay.MissileSelected)
                {
                    player.SceneAs<Level>().Add(new Missile(player, Position, false, true));
                    CurrentMissiles--;
                    BeamDelay = 0.5f;
                }
                else if (ammoDisplay.SuperMissileSelected)
                {
                    player.SceneAs<Level>().Add(new Missile(player, Position, true, true));
                    CurrentSuperMissiles--;
                    BeamDelay = 0.75f;
                }
            }
            while (BeamDelay > 0f)
            {
                BeamDelay -= Engine.DeltaTime;
                yield return null;
            }
        }

        public IEnumerator Destroy(bool normalRespawn = false, bool silence = false, bool forced = false)
        {
            if (!SaveData.Instance.Assists.Invincible || forced)
            {
                dead = true;
                Level Level = Engine.Scene as Level;
                if (FakePlayer != null)
                {
                    player.StateMachine.State = 11;
                    player.StateMachine.Locked = true;
                    player.DummyGravity = false;
                    player.Speed = Vector2.Zero;
                    Level.PauseLock = true;
                }
                Level.Displacement.AddBurst(Position, 0.3f, 0f, 80f);
                Level.Shake();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                if (!silence)
                {
                    Audio.Play("event:/game/xaphan/drone_destroy", Position);
                }
                Visible = false;
                DroneDebris.Burst(Position, Calc.HexToColor("DEAC75"), 12);
                yield return 0.5f;
                if (player != null)
                {
                    if (forced || !enabled)
                    {
                        XaphanModule.ModSession.CurrentDroneMissile = 0;
                        XaphanModule.ModSession.CurrentDroneSuperMissile = 0;
                        Level.Session.RespawnPoint = CurrentSpawn;
                        XaphanModule.fakePlayerFacing = 0;
                        XaphanModule.fakePlayerPosition = Vector2.Zero;
                        if (startRoom == Level.Session.Level)
                        {
                            DynData<Player> playerData = new(player);
                            Hitbox normalPlayerHitbox = playerData.Get<Hitbox>("normalHitbox");
                            normalPlayerHitbox.Height = 11f;
                            normalPlayerHitbox.Width = 8f;
                            normalPlayerHitbox.Left = -4f;
                            normalPlayerHitbox.Top = -11f;
                            Hitbox normalPlayerHurtbox = playerData.Get<Hitbox>("normalHurtbox");
                            normalPlayerHurtbox.Height = 9f;
                            normalPlayerHurtbox.Width = 8f;
                            normalPlayerHurtbox.Left = -4f;
                            normalPlayerHurtbox.Top = -11f;
                            Hitbox duckPlayerHitbox = playerData.Get<Hitbox>("duckHitbox");
                            duckPlayerHitbox.Height = 6f;
                            duckPlayerHitbox.Width = 8f;
                            duckPlayerHitbox.Left = -4f;
                            duckPlayerHitbox.Top = -6f;
                            Hitbox duckPlayerHurtbox = playerData.Get<Hitbox>("duckHurtbox");
                            duckPlayerHurtbox.Height = 4f;
                            duckPlayerHurtbox.Width = 8f;
                            duckPlayerHurtbox.Left = -4f;
                            duckPlayerHurtbox.Top = -6f;
                            if (FakePlayer != null && !normalRespawn)
                            {
                                player.Position = FakePlayer.Position;
                                player.DummyGravity = true;
                            }
                            if (!normalRespawn)
                            {
                                if (player != null && !player.Dead)
                                {
                                    Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(player.CameraTarget.X, player.CameraTarget.Y), 0.5f, Ease.SineInOut)));
                                }
                                if (FakePlayer != null)
                                {
                                    player.Facing = FakePlayer.Facing;
                                    FakePlayer.RemoveSelf();
                                }
                                player.Visible = true;
                                player.Light.Position = new Vector2(0f, -8f);
                                player.DummyAutoAnimate = false;
                                player.Sprite.Play("wakeUp");
                                player.Sprite.Rate = 2f;
                                while (player.Sprite.CurrentAnimationID == "wakeUp")
                                {
                                    yield return null;
                                }
                                player.StateMachine.Locked = false;
                                player.StateMachine.State = 0;
                                Level.PauseLock = false;
                            }
                        }
                        else
                        {
                            if (FakePlayer != null)
                            {
                                bool faceLeft = false;
                                if (FakePlayer.Facing == Facings.Left)
                                {
                                    faceLeft = true;
                                }
                                if (!FakePlayer.Dead)
                                {
                                    foreach (DroneSwitch droneSwitch in Level.Tracker.GetEntities<DroneSwitch>())
                                    {
                                        if (droneSwitch.wasPressed)
                                        {
                                            string Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
                                            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                                            SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_true", false);
                                            SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_false", false);
                                            if (droneSwitch.registerInSaveData && droneSwitch.saveDataOnlyAfterCheckpoint)
                                            {
                                                if (SceneAs<Level>().Session.GetFlag(droneSwitch.flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                                                {
                                                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                                                }
                                                else if (!SceneAs<Level>().Session.GetFlag(droneSwitch.flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag))
                                                {
                                                    XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + droneSwitch.flag);
                                                }
                                            }
                                        }
                                    }
                                    if (normalRespawn)
                                    {
                                        Scene.Add(new TeleportCutscene(player, startRoom, FakePlayer.Position, 0, 0, true, 0f, "Fade", respawnAnim: true, oldRespawn: true));
                                    }
                                    else
                                    {
                                        Scene.Add(new TeleportCutscene(player, startRoom, Vector2.Zero, (int)cameraPosition.X, (int)cameraPosition.Y, true, 0f, "Fade", wakeUpAnim: true, spawnPositionX: FakePlayer.Position.X, spawnPositionY: FakePlayer.Position.Y, faceLeft: faceLeft));
                                    }
                                }
                            }
                            yield return 0.5f;
                        }
                        if (!normalRespawn)
                        {
                            RemoveSelf();
                        }
                    }
                    else
                    {
                        Level.DoScreenWipe(false, delegate
                        {
                            player.StateMachine.Locked = false;
                            player.StateMachine.State = 0;
                            Level.PauseLock = false;
                            XaphanModule.startAsDrone = true;
                            XaphanModule.droneStartRoom = startRoom;
                            XaphanModule.droneCurrentSpawn = CurrentSpawn;
                            if (FakePlayer != null)
                            {
                                XaphanModule.fakePlayerSpriteFrame = FakePlayer.Sprite.CurrentAnimationFrame;
                            }
                            int chapterIndex = Level.Session.Area.ChapterIndex;
                            foreach (DroneSwitch droneSwitch in Level.Tracker.GetEntities<DroneSwitch>())
                            {
                                Level.Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_true", false);
                                Level.Session.SetFlag("Ch" + chapterIndex + "_" + droneSwitch.flag + "_false", false);
                                if (!droneSwitch.persistent && !droneSwitch.FlagRegiseredInSaveData() && droneSwitch.startSpawnPoint == Level.Session.RespawnPoint)
                                {
                                    if ((droneSwitch.registerInSaveData && droneSwitch.saveDataOnlyAfterCheckpoint) || !droneSwitch.registerInSaveData)
                                    {
                                        if (droneSwitch.flagState)
                                        {
                                            Level.Session.SetFlag(droneSwitch.flag, true);
                                        }
                                        else
                                        {
                                            Level.Session.SetFlag(droneSwitch.flag, false);
                                        }
                                    }
                                }
                            }
                            Level.Reload();
                        });
                    }
                }
            }
        }

        public override void Render()
        {
            if (!Hold.IsHeld)
            {
                droneSprite.RenderPosition = player.Position;
                droneSprite.Render();
            }
            else
            {
                base.Render();
            }
        }

        public override void DebugRender(Camera camera)
        {

        }
    }
}
