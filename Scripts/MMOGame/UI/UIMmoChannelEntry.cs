namespace MultiplayerARPG.MMO
{
    public class UIMmoChannelEntry : UISelectionEntry<ChannelEntry>
    {
        public TextWrapper textId;
        public TextWrapper textTitle;
        public UIGageValue gageConnections = new UIGageValue();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            textId = null;
            textTitle = null;
            gageConnections = null;
        }

        protected override void UpdateData()
        {
            if (textId != null)
                textId.text = Data.id;

            if (textTitle != null)
                textTitle.text = Data.title;

            gageConnections.Update(Data.connections, Data.maxConnections);
        }
    }
}
