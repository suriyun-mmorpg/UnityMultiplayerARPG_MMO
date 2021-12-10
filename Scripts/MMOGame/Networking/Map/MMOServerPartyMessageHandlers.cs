using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerPartyMessageHandlers : MonoBehaviour, IServerPartyMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }

        public ClusterClient ClusterClient
        {
            get { return (BaseGameNetworkManager.Singleton as MapNetworkManager).ClusterClient; }
        }
#endif

        public async UniTaskVoid HandleRequestAcceptPartyInvitation(RequestHandlerData requestHandler, RequestAcceptPartyInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptPartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanAcceptPartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            playerCharacter.PartyId = request.partyId;
            validateResult.Party.AddMember(playerCharacter);
            GameInstance.ServerPartyHandlers.SetParty(request.partyId, validateResult.Party);
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            // Save to database
            _ = DbServiceClient.UpdateCharacterPartyAsync(new UpdateCharacterPartyReq()
            {
                SocialCharacterData = SocialCharacterData.Create(playerCharacter),
                PartyId = request.partyId
            });
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendAddSocialMember(MMOMessageTypes.UpdatePartyMember, request.partyId, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            }
            GameInstance.ServerGameMessageHandlers.SendSetPartyData(requestHandler.ConnectionId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMembersToOne(requestHandler.ConnectionId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMemberToMembers(validateResult.Party, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_PARTY_INVITATION_ACCEPTED);
            // Response to invitee
            result.Invoke(AckResponseCode.Success, new ResponseAcceptPartyInvitationMessage()
            {
                message = UITextKeys.UI_PARTY_INVITATION_ACCEPTED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestDeclinePartyInvitation(RequestHandlerData requestHandler, RequestDeclinePartyInvitationMessage request, RequestProceedResultDelegate<ResponseDeclinePartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanDeclinePartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_PARTY_INVITATION_DECLINED);
            // Response to invitee
            result.Invoke(AckResponseCode.Success, new ResponseDeclinePartyInvitationMessage()
            {
                message = UITextKeys.UI_PARTY_INVITATION_DECLINED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestSendPartyInvitation(RequestHandlerData requestHandler, RequestSendPartyInvitationMessage request, RequestProceedResultDelegate<ResponseSendPartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            BasePlayerCharacterEntity inviteeCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out inviteeCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanSendPartyInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.AppendPartyInvitation(playerCharacter.PartyId, request.inviteeId);
            GameInstance.ServerGameMessageHandlers.SendNotifyPartyInvitation(inviteeCharacter.ConnectionId, new PartyInvitationData()
            {
                InviterId = playerCharacter.Id,
                InviterName = playerCharacter.CharacterName,
                InviterLevel = playerCharacter.Level,
                PartyId = validateResult.PartyId,
                ShareExp = validateResult.Party.shareExp,
                ShareItem = validateResult.Party.shareItem,
            });
            result.Invoke(AckResponseCode.Success, new ResponseSendPartyInvitationMessage());
#endif
        }

        public async UniTaskVoid HandleRequestCreateParty(RequestHandlerData requestHandler, RequestCreatePartyMessage request, RequestProceedResultDelegate<ResponseCreatePartyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = playerCharacter.CanCreateParty();
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            PartyResp createPartyResp = await DbServiceClient.CreatePartyAsync(new CreatePartyReq()
            {
                LeaderCharacterId = playerCharacter.Id,
                ShareExp = request.shareExp,
                ShareItem = request.shareItem
            });
            PartyData party = createPartyResp.PartyData;
            GameInstance.ServerPartyHandlers.SetParty(party.id, party);
            playerCharacter.PartyId = party.id;
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendCreateParty(MMOMessageTypes.UpdateParty, party.id, party.shareExp, party.shareItem, playerCharacter.Id);
                ClusterClient.SendAddSocialMember(MMOMessageTypes.UpdatePartyMember, party.id, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            }
            GameInstance.ServerGameMessageHandlers.SendSetPartyData(requestHandler.ConnectionId, party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMembersToOne(requestHandler.ConnectionId, party);
            result.Invoke(AckResponseCode.Success, new ResponseCreatePartyMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangePartyLeader(RequestHandlerData requestHandler, RequestChangePartyLeaderMessage request, RequestProceedResultDelegate<ResponseChangePartyLeaderMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartyLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Party.SetLeader(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            // Save to database
            _ = DbServiceClient.UpdatePartyLeaderAsync(new UpdatePartyLeaderReq()
            {
                PartyId = validateResult.PartyId,
                LeaderCharacterId = request.memberId,
            });
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendChangePartyLeader(MMOMessageTypes.UpdateParty, validateResult.PartyId, request.memberId);
            }
            GameInstance.ServerGameMessageHandlers.SendSetPartyLeaderToMembers(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartyLeaderMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangePartySetting(RequestHandlerData requestHandler, RequestChangePartySettingMessage request, RequestProceedResultDelegate<ResponseChangePartySettingMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartySetting(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Party.Setting(request.shareExp, request.shareItem);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            // Save to database
            _ = DbServiceClient.UpdatePartyAsync(new UpdatePartyReq()
            {
                PartyId = validateResult.PartyId,
                ShareExp = request.shareExp,
                ShareItem = request.shareItem
            });
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendPartySetting(MMOMessageTypes.UpdateParty, validateResult.PartyId, request.shareExp, request.shareItem);
            }
            GameInstance.ServerGameMessageHandlers.SendSetPartySettingToMembers(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartySettingMessage());
#endif
        }

        public async UniTaskVoid HandleRequestKickMemberFromParty(RequestHandlerData requestHandler, RequestKickMemberFromPartyMessage request, RequestProceedResultDelegate<ResponseKickMemberFromPartyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanKickMemberFromParty(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            IPlayerCharacterData memberCharacter;
            long memberConnectionId;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out memberCharacter) &&
                GameInstance.ServerUserHandlers.TryGetConnectionId(request.memberId, out memberConnectionId))
            {
                memberCharacter.ClearParty();
                GameInstance.ServerGameMessageHandlers.SendClearPartyData(memberConnectionId, validateResult.PartyId);
            }
            validateResult.Party.RemoveMember(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            // Save to database
            _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
            {
                CharacterId = request.memberId
            });
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, request.memberId);
            }
            GameInstance.ServerGameMessageHandlers.SendRemovePartyMemberToMembers(validateResult.Party, request.memberId);
            result.Invoke(AckResponseCode.Success, new ResponseKickMemberFromPartyMessage());
#endif
        }

        public async UniTaskVoid HandleRequestLeaveParty(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeavePartyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanLeaveParty(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // If it is leader kick all members and terminate party
            if (validateResult.Party.IsLeader(playerCharacter.Id))
            {
                IPlayerCharacterData memberCharacter;
                long memberConnectionId;
                foreach (string memberId in validateResult.Party.GetMemberIds())
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter) &&
                        GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out memberConnectionId))
                    {
                        memberCharacter.ClearParty();
                        GameInstance.ServerGameMessageHandlers.SendClearPartyData(memberConnectionId, validateResult.PartyId);
                    }
                    // Save to database
                    _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                    {
                        CharacterId = memberId
                    });
                    // Broadcast via chat server
                    if (ClusterClient.IsNetworkActive)
                    {
                        ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, memberId);
                    }
                }
                GameInstance.ServerPartyHandlers.RemoveParty(validateResult.PartyId);
                // Save to database
                _ = DbServiceClient.DeletePartyAsync(new DeletePartyReq()
                {
                    PartyId = validateResult.PartyId
                });
                // Broadcast via chat server
                if (ClusterClient.IsNetworkActive)
                {
                    ClusterClient.SendPartyTerminate(MMOMessageTypes.UpdateParty, validateResult.PartyId);
                }
            }
            else
            {
                playerCharacter.ClearParty();
                validateResult.Party.RemoveMember(playerCharacter.Id);
                GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
                GameInstance.ServerGameMessageHandlers.SendRemovePartyMemberToMembers(validateResult.Party, playerCharacter.Id);
                GameInstance.ServerGameMessageHandlers.SendClearPartyData(requestHandler.ConnectionId, validateResult.PartyId);
                // Save to database
                _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                {
                    CharacterId = playerCharacter.Id
                });
                // Broadcast via chat server
                if (ClusterClient.IsNetworkActive)
                {
                    ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, playerCharacter.Id);
                }
            }
            result.Invoke(AckResponseCode.Success, new ResponseLeavePartyMessage());
#endif
        }
    }
}
