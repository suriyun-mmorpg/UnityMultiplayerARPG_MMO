using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerGuildMessageHandlers : MonoBehaviour, IServerGuildMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }
        public IServerGuildHandlers ServerGuildHandlers { get; set; }
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

        public async UniTaskVoid HandleRequestAcceptGuildInvitation(RequestHandlerData requestHandler, RequestAcceptGuildInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildInvitationMessage()
                {
                    error = ResponseAcceptGuildInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanAcceptGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseAcceptGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundGuild:
                        error = ResponseAcceptGuildInvitationMessage.Error.GuildNotFound;
                        break;
                    case GameMessage.Type.NotFoundGuildInvitation:
                        error = ResponseAcceptGuildInvitationMessage.Error.InvitationNotFound;
                        break;
                    case GameMessage.Type.JoinedAnotherGuild:
                        error = ResponseAcceptGuildInvitationMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseAcceptGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            playerCharacter.GuildId = request.guildId;
            validateResult.Guild.AddMember(playerCharacter);
            ServerGuildHandlers.SetGuild(request.guildId, validateResult.Guild);
            ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            // Save to database
            _ = DbServiceClient.UpdateCharacterGuildAsync(new UpdateCharacterGuildReq()
            {
                SocialCharacterData = DatabaseServiceUtils.ToByteString(SocialCharacterData.Create(playerCharacter)),
                GuildId = request.guildId,
                GuildRole = validateResult.Guild.GetMemberRole(playerCharacter.Id)
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, request.guildId, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.GuildInvitationAccepted);
            BaseGameNetworkManager.Singleton.SendCreateGuildToClient(requestHandler.ConnectionId, validateResult.Guild);
            BaseGameNetworkManager.Singleton.SendAddGuildMembersToClient(requestHandler.ConnectionId, validateResult.Guild);
            BaseGameNetworkManager.Singleton.SendAddGuildMemberToClients(validateResult.Guild, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            result.Invoke(AckResponseCode.Success, new ResponseAcceptGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestDeclineGuildInvitation(RequestHandlerData requestHandler, RequestDeclineGuildInvitationMessage request, RequestProceedResultDelegate<ResponseDeclineGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildInvitationMessage()
                {
                    error = ResponseDeclineGuildInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanDeclineGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseDeclineGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundGuild:
                        error = ResponseDeclineGuildInvitationMessage.Error.GuildNotFound;
                        break;
                    case GameMessage.Type.NotFoundGuildInvitation:
                        error = ResponseDeclineGuildInvitationMessage.Error.InvitationNotFound;
                        break;
                    default:
                        error = ResponseDeclineGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            result.Invoke(AckResponseCode.Success, new ResponseDeclineGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestSendGuildInvitation(RequestHandlerData requestHandler, RequestSendGuildInvitationMessage request, RequestProceedResultDelegate<ResponseSendGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = ResponseSendGuildInvitationMessage.Error.CharacterNotFound,
                });
                return;
            }
            BasePlayerCharacterEntity inviteeCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(request.inviteeId, out inviteeCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = ResponseSendGuildInvitationMessage.Error.InviteeNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanSendGuildInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseSendGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.CannotSendGuildInvitation:
                        error = ResponseSendGuildInvitationMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterJoinedAnotherGuild:
                        error = ResponseSendGuildInvitationMessage.Error.InviteeNotAvailable;
                        break;
                    default:
                        error = ResponseSendGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            ServerGuildHandlers.AppendGuildInvitation(playerCharacter.GuildId, request.inviteeId);
            BaseGameNetworkManager.Singleton.SendNotifyGuildInvitationToClient(inviteeCharacter.ConnectionId, new GuildInvitationData()
            {
                InviterId = playerCharacter.Id,
                InviterName = playerCharacter.CharacterName,
                InviterLevel = playerCharacter.Level,
                GuildId = validateResult.GuildId,
                GuildName = validateResult.Guild.guildName,
                GuildLevel = validateResult.Guild.level,
            });
            result.Invoke(AckResponseCode.Success, new ResponseSendGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestCreateGuild(RequestHandlerData requestHandler, RequestCreateGuildMessage request, RequestProceedResultDelegate<ResponseCreateGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseCreateGuildMessage()
                {
                    error = ResponseCreateGuildMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = playerCharacter.CanCreateGuild(request.guildName);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseCreateGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.JoinedAnotherGuild:
                        error = ResponseCreateGuildMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseCreateGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseCreateGuildMessage()
                {
                    error = error,
                });
                return;
            }
            FindGuildNameResp findGuildNameResp = await DbServiceClient.FindGuildNameAsync(new FindGuildNameReq()
            {
                GuildName = request.guildName,
            });
            if (findGuildNameResp.FoundAmount > 0)
            {
                // Cannot create guild because guild name is already existed
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.ExistedGuildName);
                result.Invoke(AckResponseCode.Error, new ResponseCreateGuildMessage()
                {
                    error = ResponseCreateGuildMessage.Error.NameExisted,
                });
                return;
            }
            GuildResp createGuildResp = await DbServiceClient.CreateGuildAsync(new CreateGuildReq()
            {
                LeaderCharacterId = playerCharacter.Id,
                GuildName = request.guildName,
            });
            GuildData guild = DatabaseServiceUtils.FromByteString<GuildData>(createGuildResp.GuildData);
            GameInstance.Singleton.SocialSystemSetting.DecreaseCreateGuildResource(playerCharacter);
            ServerGuildHandlers.SetGuild(guild.id, guild);
            playerCharacter.GuildId = guild.id;
            playerCharacter.GuildRole = guild.GetMemberRole(playerCharacter.Id);
            playerCharacter.SharedGuildExp = 0;
            playerCharacter.GuildName = request.guildName;
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendCreateGuild(null, MMOMessageTypes.UpdateGuild, guild.id, request.guildName, playerCharacter.Id);
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, guild.id, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            }
            BaseGameNetworkManager.Singleton.SendCreateGuildToClient(requestHandler.ConnectionId, guild);
            BaseGameNetworkManager.Singleton.SendAddGuildMembersToClient(requestHandler.ConnectionId, guild);
            result.Invoke(AckResponseCode.Success, new ResponseCreateGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildLeader(RequestHandlerData requestHandler, RequestChangeGuildLeaderMessage request, RequestProceedResultDelegate<ResponseChangeGuildLeaderMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildLeaderMessage()
                {
                    error = ResponseChangeGuildLeaderMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanChangeGuildLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildLeaderMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildLeaderMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildLeaderMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedGuild:
                        error = ResponseChangeGuildLeaderMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseChangeGuildLeaderMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildLeaderMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetLeader(request.memberId);
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Save to database
            _ = DbServiceClient.UpdateGuildLeaderAsync(new UpdateGuildLeaderReq()
            {
                GuildId = validateResult.GuildId,
                LeaderCharacterId = request.memberId
            });
            _ = DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                MemberCharacterId = request.memberId,
                GuildRole = validateResult.Guild.GetMemberRole(request.memberId)
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendChangeGuildLeader(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.memberId);
            BaseGameNetworkManager.Singleton.SendChangeGuildLeaderToClients(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildLeaderMessage());
        }

        public async UniTaskVoid HandleRequestKickMemberFromGuild(RequestHandlerData requestHandler, RequestKickMemberFromGuildMessage request, RequestProceedResultDelegate<ResponseKickMemberFromGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromGuildMessage()
                {
                    error = ResponseKickMemberFromGuildMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanKickMemberFromGuild(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseKickMemberFromGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseKickMemberFromGuildMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.CannotKickGuildLeader:
                    case GameMessage.Type.CannotKickYourSelfFromGuild:
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseKickMemberFromGuildMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedGuild:
                        error = ResponseKickMemberFromGuildMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseKickMemberFromGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromGuildMessage()
                {
                    error = error,
                });
                return;
            }
            BasePlayerCharacterEntity memberEntity;
            if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(request.memberId, out memberEntity))
            {
                memberEntity.ClearGuild();
                BaseGameNetworkManager.Singleton.SendGuildTerminateToClient(memberEntity.ConnectionId, validateResult.GuildId);
            }
            validateResult.Guild.RemoveMember(request.memberId);
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Save to database
            _ = DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
            {
                CharacterId = request.memberId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, request.memberId);
            BaseGameNetworkManager.Singleton.SendRemoveGuildMemberToClients(validateResult.Guild, request.memberId);
            result.Invoke(AckResponseCode.Success, new ResponseKickMemberFromGuildMessage());
        }

        public async UniTaskVoid HandleRequestLeaveGuild(RequestHandlerData requestHandler, RequestLeaveGuildMessage request, RequestProceedResultDelegate<ResponseLeaveGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeaveGuildMessage()
                {
                    error = ResponseLeaveGuildMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanLeaveGuild(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseLeaveGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseLeaveGuildMessage.Error.NotJoined;
                        break;
                    default:
                        error = ResponseLeaveGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseLeaveGuildMessage()
                {
                    error = error,
                });
                return;
            }
            // If it is leader kick all members and terminate guild
            if (validateResult.Guild.IsLeader(playerCharacter))
            {
                BasePlayerCharacterEntity memberEntity;
                foreach (string memberId in validateResult.Guild.GetMemberIds())
                {
                    if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(memberId, out memberEntity))
                    {
                        memberEntity.ClearGuild();
                        BaseGameNetworkManager.Singleton.SendGuildTerminateToClient(memberEntity.ConnectionId, validateResult.GuildId);
                    }
                    // Save to database
                    _ = DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                    {
                        CharacterId = memberId
                    });
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, memberId);
                }
                ServerGuildHandlers.RemoveGuild(validateResult.GuildId);
                // Save to database
                _ = DbServiceClient.DeleteGuildAsync(new DeleteGuildReq()
                {
                    GuildId = validateResult.GuildId
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendGuildTerminate(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId);
            }
            else
            {
                playerCharacter.ClearGuild();
                BaseGameNetworkManager.Singleton.SendGuildTerminateToClient(playerCharacter.ConnectionId, validateResult.GuildId);
                validateResult.Guild.RemoveMember(playerCharacter.Id);
                ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
                BaseGameNetworkManager.Singleton.SendRemoveGuildMemberToClients(validateResult.Guild, playerCharacter.Id);
                // Save to database
                _ = DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                {
                    CharacterId = playerCharacter.Id
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, playerCharacter.Id);
            }
            result.Invoke(AckResponseCode.Success, new ResponseLeaveGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildMessageMessage()
                {
                    error = ResponseChangeGuildMessageMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanChangeGuildMessage(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildMessageMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildMessageMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildMessageMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeGuildMessageMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildMessageMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.guildMessage = request.message;
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Save to database
            _ = DbServiceClient.UpdateGuildMessageAsync(new UpdateGuildMessageReq()
            {
                GuildId = validateResult.GuildId,
                GuildMessage = request.message
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildMessage(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.message);
            BaseGameNetworkManager.Singleton.SendSetGuildMessageToClients(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildMessageMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildRole(RequestHandlerData requestHandler, RequestChangeGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeGuildRoleMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildRoleMessage()
                {
                    error = ResponseChangeGuildRoleMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanChangeGuildRole(playerCharacter, request.guildRole, request.name);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildRoleMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildRoleMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildRoleMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeGuildRoleMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildRoleMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetRole(request.guildRole, request.name, request.canInvite, request.canKick, request.shareExpPercentage);
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Change characters guild role
            IPlayerCharacterData memberCharacter;
            foreach (string memberId in validateResult.Guild.GetMemberIds())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter))
                {
                    memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
                    // Save to database
                    _ = DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
                    {
                        MemberCharacterId = memberId,
                        GuildRole = request.guildRole,
                    });
                }
            }
            // Save to database
            _ = DbServiceClient.UpdateGuildRoleAsync(new UpdateGuildRoleReq()
            {
                GuildId = validateResult.GuildId,
                GuildRole = request.guildRole,
                RoleName = request.name,
                CanInvite = request.canInvite,
                CanKick = request.canKick,
                ShareExpPercentage = request.shareExpPercentage
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildRole(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.guildRole, request.name, request.canInvite, request.canKick, request.shareExpPercentage);
            BaseGameNetworkManager.Singleton.SendSetGuildRoleToClients(validateResult.Guild, request.guildRole, request.name, request.canInvite, request.canKick, request.shareExpPercentage);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestChangeMemberGuildRole(RequestHandlerData requestHandler, RequestChangeMemberGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeMemberGuildRoleMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeMemberGuildRoleMessage()
                {
                    error = ResponseChangeMemberGuildRoleMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanSetGuildMemberRole(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeMemberGuildRoleMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeMemberGuildRoleMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetMemberRole(request.memberId, request.guildRole);
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            IPlayerCharacterData memberCharacter;
            if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(request.memberId, out memberCharacter))
                memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
            // Save to database
            _ = DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                MemberCharacterId = request.memberId,
                GuildRole = request.guildRole
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildMemberRole(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.memberId, request.guildRole);
            BaseGameNetworkManager.Singleton.SendSetGuildMemberRoleToClients(validateResult.Guild, request.memberId, request.guildRole);
            result.Invoke(AckResponseCode.Success, new ResponseChangeMemberGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestIncreaseGuildSkillLevel(RequestHandlerData requestHandler, RequestIncreaseGuildSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseGuildSkillLevelMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseGuildSkillLevelMessage()
                {
                    error = ResponseIncreaseGuildSkillLevelMessage.Error.CharacterNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = ServerGuildHandlers.CanSetGuildMemberRole(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseIncreaseGuildSkillLevelMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseGuildSkillLevelMessage()
                {
                    error = error,
                });
                return;
            }
            // Save to database
            GuildResp resp = await DbServiceClient.AddGuildSkillAsync(new AddGuildSkillReq()
            {
                GuildId = validateResult.GuildId,
                SkillId = request.dataId
            });
            GuildData guild = resp.GuildData.FromByteString<GuildData>();
            ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendSetGuildSkillLevel(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.dataId, guild.GetSkillLevel(request.dataId));
                ChatNetworkManager.SendSetGuildGold(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, guild.gold);
                ChatNetworkManager.SendGuildLevelExpSkillPoint(null, MMOMessageTypes.UpdateGuild, validateResult.GuildId, guild.level, guild.exp, guild.skillPoint);
            }
            BaseGameNetworkManager.Singleton.SendSetGuildSkillLevelToClients(validateResult.Guild, request.dataId);
            BaseGameNetworkManager.Singleton.SendGuildLevelExpSkillPointToClients(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseGuildSkillLevelMessage());
        }
    }
}
