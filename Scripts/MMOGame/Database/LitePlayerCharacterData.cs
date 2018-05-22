using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public class LitePlayerCharacterData : IPlayerCharacterData
    {
        #region Interface Fields
        // ICharacterData
        public string id;
        public string databaseId;
        public string characterName;
        public int level;
        public int exp;
        public int currentHp;
        public int currentMp;
        public int currentFood;
        public int currentWater;
        public EquipWeapons equipWeapons;
        // IPlayerCharacterData
        public int statPoint;
        public int skillPoint;
        public int gold;
        public string currentMapName;
        public Vector3 currentPosition;
        public string respawnMapName;
        public Vector3 respawnPosition;
        public int lastUpdate;
        #endregion

        #region Interface Lists
        // ICharacterData
        public readonly List<CharacterAttribute> attributes = new List<CharacterAttribute>();
        public readonly List<CharacterSkill> skills = new List<CharacterSkill>();
        public readonly List<CharacterBuff> buffs = new List<CharacterBuff>();
        public readonly List<CharacterItem> equipItems = new List<CharacterItem>();
        public readonly List<CharacterItem> nonEquipItems = new List<CharacterItem>();
        // IPlayerCharacterData
        public readonly List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
        public readonly List<CharacterQuest> quests = new List<CharacterQuest>();
        #endregion

        #region Inteface Fields Implements
        // ICharacterData
        public string Id { get { return id; } set { id = value; } }
        public string DatabaseId { get { return databaseId; } set { databaseId = value; } }
        public string CharacterName { get { return characterName; } set { characterName = value; } }
        public int Level { get { return level; } set { level = value; } }
        public int Exp { get { return exp; } set { exp = value; } }
        public int CurrentHp { get { return currentHp; } set { currentHp = value; } }
        public int CurrentMp { get { return currentMp; } set { currentMp = value; } }
        public EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons = value; } }
        // IPlayerCharacterData
        public int StatPoint { get { return statPoint; } set { statPoint = value; } }
        public int SkillPoint { get { return skillPoint; } set { skillPoint = value; } }
        public int Gold { get { return gold; } set { gold = value; } }
        public string CurrentMapName { get { return currentMapName; } set { currentMapName = value; } }
        public Vector3 CurrentPosition { get { return currentPosition; } set { currentPosition = value; } }
        public string RespawnMapName { get { return respawnMapName; } set { respawnMapName = value; } }
        public Vector3 RespawnPosition { get { return respawnPosition; } set { respawnPosition = value; } }
        public int LastUpdate { get { return lastUpdate; } set { lastUpdate = value; } }
        #endregion

        #region Interface Lists Implements
        // ICharacterData
        public IList<CharacterAttribute> Attributes
        {
            get { return attributes; }
            set
            {
                attributes.Clear();
                attributes.AddRange(value);
            }
        }
        public IList<CharacterSkill> Skills
        {
            get { return skills; }
            set
            {
                skills.Clear();
                skills.AddRange(value);
            }
        }
        public IList<CharacterBuff> Buffs
        {
            get { return buffs; }
            set { }
        }
        public IList<CharacterItem> EquipItems
        {
            get { return equipItems; }
            set
            {
                equipItems.Clear();
                equipItems.AddRange(value);
            }
        }
        public IList<CharacterItem> NonEquipItems
        {
            get { return nonEquipItems; }
            set { }
        }
        // IPlayerCharacterData
        public IList<CharacterHotkey> Hotkeys
        {
            get { return hotkeys; }
            set { }
        }
        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set { }
        }
        #endregion
    }
}
