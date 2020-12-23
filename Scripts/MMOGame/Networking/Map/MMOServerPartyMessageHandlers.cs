using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerPartyMessageHandlers : MonoBehaviour, IServerPartyMessageHandlers
    {
        public ChatNetworkManager ChatNetworkManager { get; private set; }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        private void Awake()
        {
            ChatNetworkManager = GetComponent<ChatNetworkManager>();
        }

        public async UniTaskVoid HandleRequestAcceptPartyInvitation(RequestHandlerData requestHandler, RequestAcceptPartyInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptPartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    error = ResponseAcceptPartyInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanAcceptPartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseAcceptPartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundParty:
                        error = ResponseAcceptPartyInvitationMessage.Error.PartyNotFound;
                        break;
                    case GameMessage.Type.NotFoundPartyInvitation:
                        error = ResponseAcceptPartyInvitationMessage.Error.InvitationNotFound;
                        break;
                    case GameMessage.Type.JoinedAnotherParty:
                        error = ResponseAcceptPartyInvitationMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseAcceptPartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    error = error,
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
                SocialCharacterData = DatabaseServiceUtils.ToByteString(SocialCharacterData.Create(playerCharacter)),
                PartyId = request.partyId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, request.partyId, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.PartyInvitationAccepted);
            BaseGameNetworkManager.Singleton.SendCreatePartyToClient(requestHandler.ConnectionId, validateResult.Party);
            BaseGameNetworkManager.Singleton.SendAddPartyMembersToClient(requestHandler.ConnectionId, validateResult.Party);
            BaseGameNetworkManager.Singleton.SendAddPartyMemberToClients(validateResult.Party, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            result.Invoke(AckResponseCode.Success, new ResponseAcceptPartyInvitationMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDeclinePartyInvitation(RequestHandlerData requestHandler, RequestDeclinePartyInvitationMessage request, RequestProceedResultDelegate<ResponseDeclinePartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    error = ResponseDeclinePartyInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanDeclinePartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseDeclinePartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundParty:
                        error = ResponseDeclinePartyInvitationMessage.Error.PartyNotFound;
                        break;
                    case GameMessage.Type.NotFoundPartyInvitation:
                        error = ResponseDeclinePartyInvitationMessage.Error.InvitationNotFound;
                        break;
                    default:
                        error = ResponseDeclinePartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            result.Invoke(AckResponseCode.Success, new ResponseDeclinePartyInvitationMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSendPartyInvitation(RequestHandlerData requestHandler, RequestSendPartyInvitationMessage request, RequestProceedResultDelegate<ResponseSendPartyInvitationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = ResponseSendPartyInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            BasePlayerCharacterEntity inviteeCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out inviteeCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = ResponseSendPartyInvitationMessage.Error.InviteeNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanSendPartyInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseSendPartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.CannotSendPartyInvitation:
                        error = ResponseSendPartyInvitationMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterJoinedAnotherParty:
                        error = ResponseSendPartyInvitationMessage.Error.InviteeNotAvailable;
                        break;
                    default:
                        error = ResponseSendPartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.AppendPartyInvitation(playerCharacter.PartyId, request.inviteeId);
            BaseGameNetworkManager.Singleton.SendNotifyPartyInvitationToClient(inviteeCharacter.ConnectionId, new PartyInvitationData()
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
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    error = ResponseCreatePartyMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = playerCharacter.CanCreateParty();
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseCreatePartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.JoinedAnotherParty:
                        error = ResponseCreatePartyMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseCreatePartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    error = error,
                });
                return;
            }
            PartyResp createPartyResp = await DbServiceClient.CreatePartyAsync(new CreatePartyReq()
            {
                LeaderCharacterId = playerCharacter.Id,
                ShareExp = request.shareExp,
                ShareItem = request.shareItem
            });
            PartyData party = DatabaseServiceUtils.FromByteString<PartyData>(createPartyResp.PartyData);
            GameInstance.ServerPartyHandlers.SetParty(party.id, party);
            playerCharacter.PartyId = party.id;
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendCreateParty(null, MMOMessageTypes.UpdateParty, party.id, party.shareExp, party.shareItem, playerCharacter.Id);
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, party.id, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            }
            BaseGameNetworkManager.Singleton.SendCreatePartyToClient(requestHandler.ConnectionId, party);
            BaseGameNetworkManager.Singleton.SendAddPartyMembersToClient(requestHandler.ConnectionId, party);
            result.Invoke(AckResponseCode.Success, new ResponseCreatePartyMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangePartyLeader(RequestHandlerData requestHandler, RequestChangePartyLeaderMessage request, RequestProceedResultDelegate<ResponseChangePartyLeaderMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    error = ResponseChangePartyLeaderMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartyLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangePartyLeaderMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseChangePartyLeaderMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseChangePartyLeaderMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedParty:
                        error = ResponseChangePartyLeaderMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseChangePartyLeaderMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    error = error,
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
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendChangePartyLeader(null, MMOMessageTypes.UpdateParty, validateResult.PartyId, request.memberId);
            BaseGameNetworkManager.Singleton.SendChangePartyLeaderToClients(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartyLeaderMessage());
#endif
        }

        public async UniTaskVoid HandleRequestKickMemberFromParty(RequestHandlerData requestHandler, RequestKickMemberFromPartyMessage request, RequestProceedResultDelegate<ResponseKickMemberFromPartyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    error = ResponseKickMemberFromPartyMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanKickMemberFromParty(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseKickMemberFromPartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseKickMemberFromPartyMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.CannotKickPartyLeader:
                    case GameMessage.Type.CannotKickYourSelfFromParty:
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseKickMemberFromPartyMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedParty:
                        error = ResponseKickMemberFromPartyMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseKickMemberFromPartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    error = error,
                });
                return;
            }
            BasePlayerCharacterEntity memberEntity;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out memberEntity))
            {
                memberEntity.ClearParty();
                BaseGameNetworkManager.Singleton.SendPartyTerminateToClient(memberEntity.ConnectionId, validateResult.PartyId);
            }
            validateResult.Party.RemoveMember(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            // Save to database
            _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
            {
                CharacterId = request.memberId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, request.memberId);
            BaseGameNetworkManager.Singleton.SendRemovePartyMemberToClients(validateResult.Party, request.memberId);
            result.Invoke(AckResponseCode.Success, new ResponseKickMemberFromPartyMessage());
#endif
        }

        public async UniTaskVoid HandleRequestLeaveParty(RequestHandlerData requestHandler, RequestLeavePartyMessage request, RequestProceedResultDelegate<ResponseLeavePartyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    error = ResponseLeavePartyMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanLeaveParty(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseLeavePartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseLeavePartyMessage.Error.NotJoined;
                        break;
                    default:
                        error = ResponseLeavePartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    error = error,
                });
                return;
            }
            // If it is leader kick all members and terminate party
            if (validateResult.Party.IsLeader(playerCharacter.Id))
            {
                BasePlayerCharacterEntity memberCharacter;
                foreach (string memberId in validateResult.Party.GetMemberIds())
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter))
                    {
                        memberCharacter.ClearParty();
                        BaseGameNetworkManager.Singleton.SendPartyTerminateToClient(memberCharacter.ConnectionId, validateResult.PartyId);
                    }
                    // Save to database
                    _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                    {
                        CharacterId = memberId
                    });
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, memberId);
                }
                GameInstance.ServerPartyHandlers.RemoveParty(validateResult.PartyId);
                // Save to database
                _ = DbServiceClient.DeletePartyAsync(new DeletePartyReq()
                {
                    PartyId = validateResult.PartyId
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendPartyTerminate(null, MMOMessageTypes.UpdateParty, validateResult.PartyId);
            }
            else
            {
                playerCharacter.ClearParty();
                BaseGameNetworkManager.Singleton.SendPartyTerminateToClient(playerCharacter.ConnectionId, validateResult.PartyId);
                validateResult.Party.RemoveMember(playerCharacter.Id);
                GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
                BaseGameNetworkManager.Singleton.SendRemovePartyMemberToClients(validateResult.Party, playerCharacter.Id);
                // Save to database
                _ = DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                {
                    CharacterId = playerCharacter.Id
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, validateResult.PartyId, playerCharacter.Id);
            }
            result.Invoke(AckResponseCode.Success, new ResponseLeavePartyMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangePartySetting(RequestHandlerData requestHandler, RequestChangePartySettingMessage request, RequestProceedResultDelegate<ResponseChangePartySettingMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    error = ResponseChangePartySettingMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartySetting(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangePartySettingMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseChangePartySettingMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseChangePartySettingMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangePartySettingMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    error = error,
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
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendPartySetting(null, MMOMessageTypes.UpdateParty, validateResult.PartyId, request.shareExp, request.shareItem);
            BaseGameNetworkManager.Singleton.SendPartySettingToClients(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartySettingMessage());
#endif
        }
    }
}
