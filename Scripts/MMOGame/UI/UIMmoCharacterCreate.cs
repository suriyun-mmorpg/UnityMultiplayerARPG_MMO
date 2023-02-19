using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterCreate : UICharacterCreate
    {
        protected override void OnClickCreate()
        {
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = GenericUtils.GetUniqueId();
            characterData.SetNewPlayerCharacterData(uiInputCharacterName.text.Trim(), SelectedDataId, SelectedEntityId, SelectedFactionId);
            MMOClientInstance.Singleton.RequestCreateCharacter(characterData, OnRequestedCreateCharacter);
        }

        private void OnRequestedCreateCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseCreateCharacterMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }
    }
}
