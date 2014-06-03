
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jeden.Engine.Render;
using Jeden.Game.Physics;
using SFML.Window;
using SFML.Graphics;
using Jeden.Engine.Object;
using Jeden.Engine;
using FarseerPhysics.Dynamics;

namespace Jeden.Game
{
    class GameObjectFactory
    {

        static Dictionary<String, ConfigFileParser> ConfigFileParserMap = new Dictionary<string, ConfigFileParser>();
        static Dictionary<String, Animation> Animations = new Dictionary<String, Animation>();

        static ConfigFileParser GetConfigFileParser(String filename)
        {
            if (ConfigFileParserMap.ContainsKey(filename))
            {
                return ConfigFileParserMap[filename];
            }
            else
            {
                ConfigFileParser configFileParser = new ConfigFileParser(filename);
                ConfigFileParserMap.Add(filename, configFileParser);
                return configFileParser;
            }
        }

        static Animation GetAnimation(String filename)
        {
            if (Animations.ContainsKey(filename))
            {
                return Animations[filename].CloneFrames();
            }
            else
            {
                Animation anim = new Animation(filename);
                Animations.Add(filename, anim);
                return anim;
            }
        }

        // Creates and animation that plays once then destroys itself.
        static GameObject CreateEffect(Vector2f position, String configFilename, String animationFilename)
        {
            Animation anim = GetAnimation(animationFilename);
            ConfigFileParser configFileParser = GetConfigFileParser(configFilename);

            GameObject gameObject = new GameObject();
            gameObject.Position = position;
            GameState.GameObjects.Add(gameObject);

            AnimationRenderComponent animationRenderComponent = new AnimationRenderComponent(anim, RenderMgr, gameObject);
            RenderMgr.Components.Add(animationRenderComponent);

            animationRenderComponent.WorldWidth = configFileParser.GetAsFloat("SpriteWidth");
            animationRenderComponent.WorldHeight = configFileParser.GetAsFloat("SpriteHeight");
            animationRenderComponent.IsLooping = false;
            animationRenderComponent.ZIndex = configFileParser.GetAsInt("ZIndex");
            gameObject.AddComponent(animationRenderComponent);
            gameObject.AddComponent(new InvalidatesWhenAnimationIsFinishedComponent(gameObject));

            return gameObject;
        }

        static ConfigFileParser playerConfigFileParser = new ConfigFileParser("cfg/player.txt");
        static ConfigFileParser flyingBugConfigFileParser = new ConfigFileParser("cfg/flying_bug.txt");
        static ConfigFileParser bulletConfigFileParser = new ConfigFileParser("cfg/bullet.txt");
        static ConfigFileParser jabConfigFileParser = new ConfigFileParser("cfg/jab.txt");
        static ConfigFileParser explosionConfigFileParser = new ConfigFileParser("cfg/explosion.txt");
        static ConfigFileParser stingerConfigFileParser = new ConfigFileParser("cfg/stinger.txt");
        static ConfigFileParser meleeWeaponConfigFileParser = new ConfigFileParser("cfg/melee_weapon.txt");
        static ConfigFileParser gunWeaponConfigFileParser = new ConfigFileParser("cfg/gun_weapon.txt");

        /// <summary>
        /// Create's the main player
        /// </summary>
        /// <param name="position">The starting position of the player</param>
        /// <returns></returns>
        public static GameObject CreatePlayer(Vector2f position)
        {
            GameObject player = new GameObject();
            player.Position = position;

            AnimationSetRenderComponent anims = RenderMgr.MakeNewAnimationSetComponent(player);

            anims.AddAnimation("Walking", new Animation("cfg/player_walking_anim.txt"));
            anims.AddAnimation("Attacking", new Animation("cfg/player_melee_attacking_anim.txt"));
            anims.AddAnimation("Idle", new Animation("cfg/player_idle_anim.txt"));
            anims.AddAnimation("Jumping", new Animation("cfg/player_jumping_anim.txt"));
            anims.AddAnimation("Falling", new Animation("cfg/player_falling_anim.txt"));
            //idle
            //jumping
            //ducking
            anims.SetIsLooping("Attacking", false);
            anims.SetIsLooping("Jumping", false);

            anims.SetAnimation("Walking");
            anims.ZIndex = playerConfigFileParser.GetAsInt("ZIndex");

            anims.WorldWidth = playerConfigFileParser.GetAsFloat("SpriteWidth");
            anims.WorldHeight = playerConfigFileParser.GetAsFloat("SpriteHeight");
            player.AddComponent(anims);

            PhysicsComponent physicsComp = PhysicsMgr.MakeNewComponent(
                player,
                playerConfigFileParser.GetAsFloat("CollisionWidth"),
                playerConfigFileParser.GetAsFloat("CollisionHeight"), 
                PhysicsManager.PlayerCategory, 
                PhysicsManager.EnemyCategory | PhysicsManager.MapCategory, 
                BodyType.Dynamic);


            physicsComp.Body.Friction = playerConfigFileParser.GetAsFloat("Friction");
            physicsComp.Body.GravityScale = playerConfigFileParser.GetAsFloat("GravityScale");
            physicsComp.Body.Mass = playerConfigFileParser.GetAsFloat("Mass");
            physicsComp.Body.Restitution = playerConfigFileParser.GetAsFloat("Restitution");

            player.AddComponent(physicsComp);

            RectRenderComponent collisionRect = new RectRenderComponent(playerConfigFileParser.GetAsFloat("CollisionWidth"),
                playerConfigFileParser.GetAsFloat("CollisionHeight"), new Color(255, 0, 0, 100), RenderMgr, player);
            RenderMgr.Components.Add(collisionRect);
            player.AddComponent(collisionRect);

            CharacterControllerComponent controller = new CharacterControllerComponent(anims, physicsComp, RenderMgr.Camera, player);
            controller.InAirMovementImpulse = playerConfigFileParser.GetAsFloat("InAirMovementImpulse");
            controller.WalkImpulse = playerConfigFileParser.GetAsFloat("WalkImpulse");
            controller.JumpImpulse = playerConfigFileParser.GetAsFloat("JumpImpulse");
            controller.WalkingLinearDamping = playerConfigFileParser.GetAsFloat("WalkingLinearDamping");
            controller.InAirLinearDamping = playerConfigFileParser.GetAsFloat("InAirLinearDamping");

            player.AddComponent(controller);

            player.AddComponent(new HealthComponent(player, playerConfigFileParser.GetAsFloat("MaxHealth"), 100));

            GameState.GameObjects.Add(player);

            return player;
        }


        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        /// <param name="attacker">The GameObject that fired the bullet</param>
        /// <param name="position">The starting position of the bullet</param>
        /// <param name="direction">The direction in which the bullet was fired[needs to be unit length]</param>
        /// <returns></returns>
        public static GameObject CreateBullet(GameObject attacker, Vector2f position, int direction)
        {

            GameObject gameObject = new GameObject();
            GameState.GameObjects.Add(gameObject);
            gameObject.Position = position;

            PhysicsComponent physicsComp = PhysicsMgr.MakeNewComponent(gameObject, 
                bulletConfigFileParser.GetAsFloat("CollisionWidth"), 
                bulletConfigFileParser.GetAsFloat("CollisionHeight"),
                PhysicsManager.PlayerCategory, PhysicsManager.EnemyCategory | PhysicsManager.MapCategory, BodyType.Dynamic);
            physicsComp.Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(direction, 0) * bulletConfigFileParser.GetAsFloat("Speed");
            physicsComp.Body.GravityScale = bulletConfigFileParser.GetAsFloat("GravityScale");
            physicsComp.Body.LinearDamping = 0.0f;
            gameObject.AddComponent(physicsComp);


            AttackComponent attackComp = new AttackComponent(attacker, bulletConfigFileParser.GetAsFloat("Damage"), gameObject);
            gameObject.AddComponent(attackComp);

            AnimationRenderComponent renderComp = RenderMgr.MakeNewAnimationComponent("cfg/bullet_anim.txt", gameObject);
            renderComp.WorldWidth = bulletConfigFileParser.GetAsFloat("SpriteWidth");
            renderComp.WorldHeight = bulletConfigFileParser.GetAsFloat("SpriteHeight");
            renderComp.ZIndex = bulletConfigFileParser.GetAsInt("ZIndex");
            if (direction < 0)
                renderComp.FlipX = true;
            gameObject.AddComponent(renderComp);


            gameObject.AddComponent(new ExplodesOnCollisionComponent(gameObject));

            RectRenderComponent collisionRect = new RectRenderComponent(bulletConfigFileParser.GetAsFloat("CollisionWidth"),
    bulletConfigFileParser.GetAsFloat("CollisionHeight"), new Color(255, 0, 0, 100), RenderMgr, gameObject);
            RenderMgr.Components.Add(collisionRect);
            gameObject.AddComponent(collisionRect);

            return gameObject;
        }

        static Texture SwordTexture = new Texture("assets/test2.png");
        /// <summary>
        /// Creates a new jab attack instance
        /// </summary>
        public static GameObject CreateJab(GameObject attacker, MeleeWeaponComponent meleeWeapon, Vector2f position, int xdirection)
        {
            float width = jabConfigFileParser.GetAsFloat("CollisionWidth");
            float height = jabConfigFileParser.GetAsFloat("CollisionHeight");
            float damage = jabConfigFileParser.GetAsFloat("Damage");
            float forwardX = jabConfigFileParser.GetAsFloat("ForwardX");
            float backwardX = jabConfigFileParser.GetAsFloat("BackwardX");
            float forwardTime = jabConfigFileParser.GetAsFloat("ForwardTime");
            float backwardTime = jabConfigFileParser.GetAsFloat("BackwardTime");

            GameObject gameObject = new GameObject();
            GameState.GameObjects.Add(gameObject);
            gameObject.Position = position;

            PhysicsComponent physicsComp = PhysicsMgr.MakeNewComponent(gameObject, width, height,  
            PhysicsManager.PlayerCategory, PhysicsManager.EnemyCategory | PhysicsManager.MapCategory, BodyType.Kinematic);
            physicsComp.Body.GravityScale = 0.0f;
            physicsComp.Body.IsSensor = true;
            
            gameObject.AddComponent(physicsComp);

            AttackComponent attackComp = new AttackComponent(attacker, damage, gameObject);
            gameObject.AddComponent(attackComp);

            SpriteRenderComponent renderComp = RenderMgr.MakeNewSpriteComponent(gameObject, SwordTexture);
            renderComp.WorldPosition = position;
            renderComp.WorldWidth = width;
            renderComp.WorldHeight = height;
            renderComp.ZIndex = 100;
            gameObject.AddComponent(renderComp);

            
            JabControllerComponent scc = new JabControllerComponent(physicsComp, meleeWeapon, 
                new Vector2f(forwardX * xdirection, 0), 
                new Vector2f(backwardX * xdirection, 0), 
                forwardTime, 
                backwardTime, 
                gameObject);
            
            gameObject.AddComponent(scc);

            RectRenderComponent collisionRect = new RectRenderComponent(
                jabConfigFileParser.GetAsFloat("CollisionWidth"),
                jabConfigFileParser.GetAsFloat("CollisionHeight"), 
                new Color(255, 0, 0, 100), 
                RenderMgr, 
                gameObject);
            RenderMgr.Components.Add(collisionRect);
            gameObject.AddComponent(collisionRect);

            return gameObject;
        }

        public static GameObject CreateMeleeWeapon(GameObject player)
        {
            GameObject gameObject = new GameObject();
            Vector2f offset = new Vector2f(meleeWeaponConfigFileParser.GetAsFloat("OffsetX"), meleeWeaponConfigFileParser.GetAsFloat("OffsetY"));
            MeleeWeaponComponent meleeWeaponComponent = new MeleeWeaponComponent(player, offset, gameObject);
            gameObject.AddComponent(meleeWeaponComponent);
            GameState.GameObjects.Add(gameObject);

            meleeWeaponComponent.AttackDelay = meleeWeaponConfigFileParser.GetAsFloat("AttackDelay");
            return gameObject;
        }

        public static GameObject CreateGunWeapon(GameObject player)
        {
            GameObject gameObject = new GameObject();

            Vector2f offset = new Vector2f(gunWeaponConfigFileParser.GetAsFloat("OffsetX"), gunWeaponConfigFileParser.GetAsFloat("OffsetY"));
            GunWeaponComponent gunWeaponComponent = new GunWeaponComponent(player, offset, gameObject);
            gameObject.AddComponent(gunWeaponComponent);
            GameState.GameObjects.Add(gameObject);
            gunWeaponComponent.AttackDelay = gunWeaponConfigFileParser.GetAsFloat("AttackDelay");

            return gameObject;
        }


        static Texture StingerTexture = new Texture("assets/stinger.png");
        /// <summary>
        /// Creates a new Bullet
        /// </summary>
        /// <param name="attacker">The GameObject that fired the bullet</param>
        /// <param name="position">The starting position of the bullet</param>
        /// <param name="direction">The direction in which the bullet was fired[needs to be unit length]</param>
        /// <returns></returns>
        public static GameObject CreateStinger(GameObject attacker, Vector2f position, Vector2f direction)
        {

            
            GameObject gameObject = new GameObject();
            GameState.GameObjects.Add(gameObject);
            gameObject.Position = position;

            PhysicsComponent physicsComp = PhysicsMgr.MakeNewComponent(
                gameObject, 
                stingerConfigFileParser.GetAsFloat("CollisionWidth"),
                stingerConfigFileParser.GetAsFloat("CollisionHeight"),
                PhysicsManager.EnemyCategory, 
                PhysicsManager.PlayerCategory | PhysicsManager.MapCategory, 
                BodyType.Dynamic);
            physicsComp.Body.LinearVelocity = 
                new Microsoft.Xna.Framework.Vector2(direction.X, direction.Y) * stingerConfigFileParser.GetAsFloat("Speed");
            physicsComp.Body.LinearDamping = 0.0f;
            physicsComp.Body.IgnoreGravity = true;
            gameObject.AddComponent(physicsComp);


            AttackComponent attackComp = new AttackComponent(attacker, stingerConfigFileParser.GetAsFloat("Damage"), gameObject);
            gameObject.AddComponent(attackComp);

            SpriteRenderComponent renderComp = RenderMgr.MakeNewSpriteComponent(gameObject, StingerTexture);
            renderComp.WorldWidth = stingerConfigFileParser.GetAsFloat("SpriteWidth");
            renderComp.WorldHeight = stingerConfigFileParser.GetAsFloat("SpriteHeight");
            renderComp.ZIndex = stingerConfigFileParser.GetAsInt("ZIndex");
            gameObject.AddComponent(renderComp);

            gameObject.AddComponent(new ExplodesOnCollisionComponent(gameObject));

            RectRenderComponent collisionRect = new RectRenderComponent(stingerConfigFileParser.GetAsFloat("CollisionWidth"),
stingerConfigFileParser.GetAsFloat("CollisionHeight"), new Color(255, 0, 0, 100), RenderMgr, gameObject);
            RenderMgr.Components.Add(collisionRect);
            gameObject.AddComponent(collisionRect);

            return gameObject;
        }
        
        static Texture EnemyTexture = new Texture("assets/bee.png");
        /// <summary>
        /// Create's an enemy
        /// </summary>
        /// <param name="position">the starting position of the enemy</param>
        /// <returns></returns>
        public static GameObject CreateFlyingBug(Vector2f position)
        {

            GameObject enemy = new GameObject();
            GameState.GameObjects.Add(enemy);
            enemy.Position = position;
            AnimationSetRenderComponent animationRenderComponent = RenderMgr.MakeNewAnimationSetComponent(enemy);

            animationRenderComponent.AddFrame("Flying", EnemyTexture);
            animationRenderComponent.WorldWidth = flyingBugConfigFileParser.GetAsFloat("SpriteWidth");
            animationRenderComponent.WorldHeight = flyingBugConfigFileParser.GetAsFloat("SpriteHeight");
            animationRenderComponent.ZIndex = flyingBugConfigFileParser.GetAsInt("ZIndex");
            
            animationRenderComponent.SetFrameTime("Flying", 2.0f);
            animationRenderComponent.SetAnimation("Flying");
            enemy.AddComponent(animationRenderComponent);

            enemy.AddComponent(new HealthComponent(enemy, flyingBugConfigFileParser.GetAsFloat("Health"), 0));

            PhysicsComponent physicsComp = PhysicsMgr.MakeNewComponent(enemy, 
                flyingBugConfigFileParser.GetAsFloat("CollisionWidth"),
                flyingBugConfigFileParser.GetAsFloat("CollisionHeight"), 
                PhysicsManager.EnemyCategory, PhysicsManager.PlayerCategory | PhysicsManager.MapCategory, 
                BodyType.Dynamic);
            physicsComp.Body.Mass = flyingBugConfigFileParser.GetAsFloat("Mass");
            enemy.AddComponent(physicsComp);

            FlyingBugControllerComponent charControllerComp = new FlyingBugControllerComponent(
                animationRenderComponent, 
                physicsComp, 
                flyingBugConfigFileParser.GetAsFloat("MovementImpulse"),
                enemy);
            enemy.AddComponent(charControllerComp);

            FlyingBugAIComponent flyingBugAIComponent = new FlyingBugAIComponent(enemy.Position, enemy);
            enemy.AddComponent(flyingBugAIComponent);

            RectRenderComponent collisionRect = new RectRenderComponent(flyingBugConfigFileParser.GetAsFloat("CollisionWidth"),
flyingBugConfigFileParser.GetAsFloat("CollisionHeight"), new Color(255, 0, 0, 100), RenderMgr, enemy);
            RenderMgr.Components.Add(collisionRect);
            enemy.AddComponent(collisionRect);

            return enemy;
        }


        static public GameObject CreateExplosion(Vector2f position)
        {
            return CreateEffect(position, "cfg/bullet_hit_effect.txt", "cfg/bullet_hit_effect_anim.txt");
        }

        public static GameObject CreateDeadPlayer(Vector2f position)
        {
            return CreateEffect(position, "cfg/dead_player.txt", "cfg/player_death_anim.txt");
        }

        public static GameObject CreateDeadFlyingBug(Vector2f position)
        {
            return CreateEffect(position, "cfg/dead_flying_bug.txt", "cfg/flying_bug_death_anim.txt");

        }

        public static GameObject CreateShieldDamageEffect(Vector2f position)
        {
            return CreateEffect(position, "cfg/shield_damage_effect.txt", "cfg/shield_anim.txt");
        }

        public static GameObject CreateGunBarellEffect(Vector2f position)
        {
            return CreateEffect(position, "cfg/gun_barell_effect.txt", "cfg/gun_barell_effect_anim.txt");
        }


        public static RenderManager RenderMgr;
        public static PhysicsManager PhysicsMgr;
        public static JedenGameState GameState;
    }
}
