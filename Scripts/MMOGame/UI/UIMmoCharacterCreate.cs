using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterCreate : UICharacterCreate
    {
        protected override void OnClickCreate()
        {
            if (buttonCreate)
                buttonCreate.interactable = false;
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = GenericUtils.GetUniqueId();
            characterData.SetNewPlayerCharacterData(uiInputCharacterName.text.Trim(), SelectedDataId, SelectedEntityId, SelectedFactionId);
#if !DISABLE_CUSTOM_CHARACTER_DATA
            characterData.PublicBools = PublicBools;
            characterData.PublicInts = PublicInts;
            characterData.PublicFloats = PublicFloats;
#endif
            MMOClientInstance.Singleton.RequestCreateCharacter(characterData, OnRequestedCreateCharacter);
        }

        private void OnRequestedCreateCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseCreateCharacterMessage response)
        {
            if (buttonCreate)
                buttonCreate.interactable = true;
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }
    }
}
