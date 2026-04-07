namespace Game070v2_NanoLab
{
    public enum TechPath { Efficiency, Growth, Prestige, Fusion }
    public enum TechEffect { ClickPower, AutoRate, PrestigeBonus, MutationControl, FusionUnlock }

    [System.Serializable]
    public class TechNodeData
    {
        public string id;
        public string nameJP;
        public string description;
        public long cost;
        public TechEffect effect;
        public float value;
        public string prerequisiteId;
        public TechPath path;
        public bool unlocked;
        public bool available; // prerequisite met and can afford
    }
}
