using BrainEaters.Configs;

namespace BrainEaters.GameFlow
{
    public static class LevelSession
    {
        public static CampaignConfig ActiveCampaign { get; private set; }
        public static LevelConfig SelectedLevel { get; private set; }

        public static void SetCampaign(CampaignConfig campaign)
        {
            ActiveCampaign = campaign;
        }

        public static void SelectLevel(CampaignConfig campaign, LevelConfig levelConfig)
        {
            ActiveCampaign = campaign;
            SelectedLevel = levelConfig;
        }

        public static void ClearSelectedLevel()
        {
            SelectedLevel = null;
        }
    }
}
