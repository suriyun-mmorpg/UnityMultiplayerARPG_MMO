using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerGuildMessageHandlers : MonoBehaviour, IServerGuildMessageHandlers
    {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }

        public ClusterClient ClusterClient
        {
            get { return (BaseGameNetworkManager.Singleton as MapNetworkManager).ClusterClient; }
        }
#endif

#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public async UniTask<UITextKeys> CreateGuildMember(GuildData guild, SocialCharacterData playerCharacter, long notifyConnectionId = -1)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            guild.AddMember(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(guild.id, guild);
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(guild.id, playerCharacter.id);
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateCharacterGuildAsync(new UpdateCharacterGuildReq()
            {
                SocialCharacterData = playerCharacter,
                GuildId = guild.id,
                GuildRole = guild.GetMemberRole(playerCharacter.id)
            });
            if (!updateResp.IsSuccess)
            {
                return UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR;
            }
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendAddSocialMember(MMOMessageTypes.UpdateGuildMember, guild.id, playerCharacter);
            }
            if (notifyConnectionId >= 0)
            {
                GameInstance.ServerGameMessageHandlers.SendSetFullGuildData(notifyConnectionId, guild);
                GameInstance.ServerGameMessageHandlers.SendAddGuildMembersToOne(notifyConnectionId, guild);
            }
            GameInstance.ServerGameMessageHandlers.SendAddGuildMemberToMembers(guild, playerCharacter);
#endif
            return UITextKeys.NONE;
        }
#endif

        public async UniTaskVoid HandleRequestAcceptGuildInvitation(RequestHandlerData requestHandler, RequestAcceptGuildInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptGuildInvitationMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseAcceptGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanAcceptGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseAcceptGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            playerCharacter.GuildId = request.guildId;
            UITextKeys createGuildMemberMessage = await CreateGuildMember(validateResult.Guild, SocialCharacterData.Create(playerCharacter), requestHandler.ConnectionId);
            if (createGuildMemberMessage.IsError())
            {
                result.InvokeError(new ResponseAcceptGuildInvitationMessage()
                {
                    message = createGuildMemberMessage,
                });
                return;
            }
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_GUILD_INVITATION_ACCEPTED);
            // Update member count
            await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
            {
                GuildId = validateResult.GuildId,
                MaxGuildMember = validateResult.Guild.MaxMember(),
            });
            // Response to invitee
            result.InvokeSuccess(new ResponseAcceptGuildInvitationMessage()
            {
                message = UITextKeys.UI_GUILD_INVITATION_ACCEPTED,
            });
#endif
        }

        public UniTaskVoid HandleRequestDeclineGuildInvitation(RequestHandlerData requestHandler, RequestDeclineGuildInvitationMessage request, RequestProceedResultDelegate<ResponseDeclineGuildInvitationMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseDeclineGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanDeclineGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseDeclineGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return default;
            }
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_GUILD_INVITATION_DECLINED);
            // Response to invitee
            result.InvokeSuccess(new ResponseDeclineGuildInvitationMessage()
            {
                message = UITextKeys.UI_GUILD_INVITATION_DECLINED,
            });
#endif
            return default;
        }

        public UniTaskVoid HandleRequestSendGuildInvitation(RequestHandlerData requestHandler, RequestSendGuildInvitationMessage request, RequestProceedResultDelegate<ResponseSendGuildInvitationMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out BasePlayerCharacterEntity inviteeCharacter))
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                });
                return default;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanSendGuildInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return default;
            }
            GameInstance.ServerGuildHandlers.AppendGuildInvitation(playerCharacter.GuildId, request.inviteeId);
            GameInstance.ServerGameMessageHandlers.SendNotifyGuildInvitation(inviteeCharacter.ConnectionId, new GuildInvitationData()
            {
                InviterId = playerCharacter.Id,
                InviterName = playerCharacter.CharacterName,
                InviterLevel = playerCharacter.Level,
                GuildId = validateResult.GuildId,
                GuildName = validateResult.Guild.guildName,
                GuildLevel = validateResult.Guild.level,
            });
            result.InvokeSuccess(new ResponseSendGuildInvitationMessage());
#endif
            return default;
        }

        public async UniTaskVoid HandleRequestCreateGuild(RequestHandlerData requestHandler, RequestCreateGuildMessage request, RequestProceedResultDelegate<ResponseCreateGuildMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = playerCharacter.CanCreateGuild(request.guildName);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            string guildName = request.guildName.Trim();
            if (!NameExtensions.IsValidGuildName(guildName))
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_GUILD_NAME
                });
                return;
            }
            DatabaseApiResult<FindGuildNameResp> findGuildNameResp = await DatabaseClient.FindGuildNameAsync(new FindGuildNameReq()
            {
                GuildName = guildName,
            });
            if (!findGuildNameResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (findGuildNameResp.Response.FoundAmount > 0)
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_GUILD_NAME_EXISTED,
                });
                return;
            }
            DatabaseApiResult<GuildResp> createGuildResp = await DatabaseClient.CreateGuildAsync(new CreateGuildReq()
            {
                LeaderCharacterId = playerCharacter.Id,
                GuildName = guildName,
            });
            if (!createGuildResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            GuildData guild = createGuildResp.Response.GuildData;
            GameInstance.Singleton.SocialSystemSetting.DecreaseCreateGuildResource(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(guild.id, guild);
            playerCharacter.GuildId = guild.id;
            playerCharacter.GuildRole = guild.GetMemberRole(playerCharacter.Id);
            playerCharacter.SharedGuildExp = 0;
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendCreateGuild(MMOMessageTypes.UpdateGuild, guild.id, guildName, playerCharacter.Id);
                ClusterClient.SendAddSocialMember(MMOMessageTypes.UpdateGuildMember, guild.id, SocialCharacterData.Create(playerCharacter));
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildData(requestHandler.ConnectionId, guild);
            GameInstance.ServerGameMessageHandlers.SendAddGuildMembersToOne(requestHandler.ConnectionId, guild);
            await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
            {
                GuildId = guild.id,
                MaxGuildMember = guild.MaxMember(),
            });
            result.InvokeSuccess(new ResponseCreateGuildMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildLeader(RequestHandlerData requestHandler, RequestChangeGuildLeaderMessage request, RequestProceedResultDelegate<ResponseChangeGuildLeaderMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildLeaderAsync(new UpdateGuildLeaderReq()
            {
                GuildId = validateResult.GuildId,
                LeaderCharacterId = request.memberId
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            DatabaseApiResult<GuildResp> updateRoleResp = await DatabaseClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                GuildId = validateResult.GuildId,
                MemberCharacterId = request.memberId,
                GuildRole = validateResult.Guild.GetMemberRole(request.memberId)
            });
            if (!updateRoleResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Update cache
            byte swappingGuildRole = validateResult.Guild.GetMemberRole(request.memberId);
            validateResult.Guild.SetLeader(request.memberId);
            validateResult.Guild.SetMemberRole(playerCharacter.Id, swappingGuildRole);
            playerCharacter.GuildRole = swappingGuildRole;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendChangeGuildLeader(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.memberId);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildLeaderToMembers(validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(validateResult.Guild, request.memberId, 0);
            result.InvokeSuccess(new ResponseChangeGuildLeaderMessage());
#endif
        }

        public async UniTaskVoid HandleRequestKickMemberFromGuild(RequestHandlerData requestHandler, RequestKickMemberFromGuildMessage request, RequestProceedResultDelegate<ResponseKickMemberFromGuildMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseKickMemberFromGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanKickMemberFromGuild(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseKickMemberFromGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult updateResp = await DatabaseClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
            {
                CharacterId = request.memberId
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseKickMemberFromGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Delete from cache
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out IPlayerCharacterData memberCharacter) &&
                GameInstance.ServerUserHandlers.TryGetConnectionId(request.memberId, out long memberConnectionId))
            {
                memberCharacter.ClearGuild();
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberConnectionId, validateResult.GuildId);
            }
            validateResult.Guild.RemoveMember(request.memberId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, request.memberId);
            }
            GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, request.memberId);
            await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
            {
                GuildId = validateResult.GuildId,
                MaxGuildMember = validateResult.Guild.MaxMember(),
            });
            result.InvokeSuccess(new ResponseKickMemberFromGuildMessage());
#endif
        }

        public async UniTaskVoid HandleRequestLeaveGuild(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeaveGuildMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseLeaveGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanLeaveGuild(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseLeaveGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            if (validateResult.Guild.IsLeader(playerCharacter.Id))
            {
                // Delete from database
                DatabaseApiResult deleteResp = await DatabaseClient.DeleteGuildAsync(new DeleteGuildReq()
                {
                    GuildId = validateResult.GuildId
                });
                if (!deleteResp.IsSuccess)
                {
                    result.InvokeError(new ResponseLeaveGuildMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
                IPlayerCharacterData memberCharacter;
                long memberConnectionId;
                foreach (string memberId in validateResult.Guild.GetMemberIds())
                {
                    // Save to database
                    _ = DatabaseClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                    {
                        CharacterId = memberId
                    });
                    // Update cache
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter) &&
                        GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out memberConnectionId))
                    {
                        memberCharacter.ClearGuild();
                        GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberConnectionId, validateResult.GuildId);
                    }
                    // Broadcast via chat server
                    if (ClusterClient.IsNetworkActive)
                    {
                        ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, memberId);
                    }
                }
                GameInstance.ServerGuildHandlers.RemoveGuild(validateResult.GuildId);
                // Broadcast via chat server
                if (ClusterClient.IsNetworkActive)
                {
                    ClusterClient.SendGuildTerminate(MMOMessageTypes.UpdateGuild, validateResult.GuildId);
                }
            }
            else
            {
                // Save to database
                DatabaseApiResult updateResp = await DatabaseClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                {
                    CharacterId = playerCharacter.Id
                });
                if (!updateResp.IsSuccess)
                {
                    result.InvokeError(new ResponseLeaveGuildMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
                // Update cache
                playerCharacter.ClearGuild();
                validateResult.Guild.RemoveMember(playerCharacter.Id);
                GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
                GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, playerCharacter.Id);
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(requestHandler.ConnectionId, validateResult.GuildId);
                // Broadcast via chat server
                if (ClusterClient.IsNetworkActive)
                {
                    ClusterClient.SendRemoveSocialMember(MMOMessageTypes.UpdateGuildMember, validateResult.GuildId, playerCharacter.Id);
                }
            }
            await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
            {
                GuildId = validateResult.GuildId,
                MaxGuildMember = validateResult.Guild.MaxMember(),
            });
            result.InvokeSuccess(new ResponseLeaveGuildMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMessage(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildMessageAsync(new UpdateGuildMessageReq()
            {
                GuildId = validateResult.GuildId,
                GuildMessage = request.message
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.guildMessage = request.message;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildMessage(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.message);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildMessageToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildMessageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage2(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMessage2(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildMessage2Async(new UpdateGuildMessageReq()
            {
                GuildId = validateResult.GuildId,
                GuildMessage = request.message
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.guildMessage2 = request.message;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildMessage2(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.message);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildMessage2ToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildMessageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildOptions(RequestHandlerData requestHandler, RequestChangeGuildOptionsMessage request, RequestProceedResultDelegate<ResponseChangeGuildOptionsMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildOptionsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildOptions(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildOptionsMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildOptionsAsync(new UpdateGuildOptionsReq()
            {
                GuildId = validateResult.GuildId,
                Options = request.options
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildOptionsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.options = request.options;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildOptions(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.options);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildOptionsToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildOptionsMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildAutoAcceptRequests(RequestHandlerData requestHandler, RequestChangeGuildAutoAcceptRequestsMessage request, RequestProceedResultDelegate<ResponseChangeGuildAutoAcceptRequestsMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildAutoAcceptRequestsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildOptions(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildAutoAcceptRequestsMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildAutoAcceptRequestsAsync(new UpdateGuildAutoAcceptRequestsReq()
            {
                GuildId = validateResult.GuildId,
                AutoAcceptRequests = request.autoAcceptRequests
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildAutoAcceptRequestsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.autoAcceptRequests = request.autoAcceptRequests;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildAutoAcceptRequests(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.autoAcceptRequests);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildAutoAcceptRequestsToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildAutoAcceptRequestsMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeGuildRole(RequestHandlerData requestHandler, RequestChangeGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeGuildRoleMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (request.guildRoleData.shareExpPercentage > GameInstance.Singleton.SocialSystemSetting.MaxShareExpPercentage)
                request.guildRoleData.shareExpPercentage = GameInstance.Singleton.SocialSystemSetting.MaxShareExpPercentage;
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildRole(playerCharacter, request.guildRole, request.guildRoleData.roleName);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildRoleMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildRoleAsync(new UpdateGuildRoleReq()
            {
                GuildId = validateResult.GuildId,
                GuildRole = request.guildRole,
                GuildRoleData = request.guildRoleData,
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.SetRole(request.guildRole, request.guildRoleData);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Change characters guild role
            IPlayerCharacterData memberCharacter;
            foreach (string memberId in validateResult.Guild.GetMemberIds())
            {
                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter))
                {
                    if (validateResult.Guild.GetMemberRole(memberCharacter.Id) == request.guildRole)
                        memberCharacter.SharedGuildExp = request.guildRoleData.shareExpPercentage;
                }
            }
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildRole(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.guildRole, request.guildRoleData);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildRoleToMembers(validateResult.Guild, request.guildRole, request.guildRoleData);
            result.InvokeSuccess(new ResponseChangeGuildRoleMessage());
#endif
        }

        public async UniTaskVoid HandleRequestChangeMemberGuildRole(RequestHandlerData requestHandler, RequestChangeMemberGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeMemberGuildRoleMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeMemberGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMemberRole(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeMemberGuildRoleMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                GuildId = validateResult.GuildId,
                MemberCharacterId = request.memberId,
                GuildRole = request.guildRole
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseChangeMemberGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update cache
            validateResult.Guild.SetMemberRole(request.memberId, request.guildRole);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out IPlayerCharacterData memberCharacter))
            {
                memberCharacter.GuildRole = request.guildRole;
                memberCharacter.SharedGuildExp = validateResult.Guild.GetRole(request.guildRole).shareExpPercentage;
            }
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildMemberRole(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.memberId, request.guildRole);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(validateResult.Guild, request.memberId, request.guildRole);
            result.InvokeSuccess(new ResponseChangeMemberGuildRoleMessage());
#endif
        }

        public async UniTaskVoid HandleRequestIncreaseGuildSkillLevel(RequestHandlerData requestHandler, RequestIncreaseGuildSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseGuildSkillLevelMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseGuildSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanIncreaseGuildSkillLevel(playerCharacter, request.dataId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseIncreaseGuildSkillLevelMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            // Save to database
            DatabaseApiResult<GuildResp> updateResp = await DatabaseClient.AddGuildSkillAsync(new AddGuildSkillReq()
            {
                GuildId = validateResult.GuildId,
                SkillId = request.dataId,
            });
            if (!updateResp.IsSuccess)
            {
                result.InvokeError(new ResponseIncreaseGuildSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            GuildData guild = updateResp.Response.GuildData;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildSkillLevel(MMOMessageTypes.UpdateGuild, validateResult.GuildId, request.dataId, guild.GetSkillLevel(request.dataId));
                ClusterClient.SendSetGuildLevelExpSkillPoint(MMOMessageTypes.UpdateGuild, validateResult.GuildId, guild.level, guild.exp, guild.skillPoint);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildSkillLevelToMembers(guild, request.dataId);
            GameInstance.ServerGameMessageHandlers.SendSetGuildLevelExpSkillPointToMembers(guild);
            result.InvokeSuccess(new ResponseIncreaseGuildSkillLevelMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSendGuildRequest(RequestHandlerData requestHandler, RequestSendGuildRequestMessage request, RequestProceedResultDelegate<ResponseSendGuildRequestMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            // Find is there is a cached guild data or not
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(request.guildId, out GuildData guild))
            {
                // No guild data is cached, try get it from database
                DatabaseApiResult<GuildResp> readGuildResp = await DatabaseClient.GetGuildAsync(new GetGuildReq()
                {
                    GuildId = request.guildId,
                });
                if (!readGuildResp.IsSuccess)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
                guild = readGuildResp.Response.GuildData;
                GameInstance.ServerGuildHandlers.SetGuild(request.guildId, guild);
            }

            // Validate that character can send guild request to join guild or not
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanSendGuildRequest(playerCharacter, request.guildId);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }

            // Auto accept requests, so add guild member automatically
            if (guild.autoAcceptRequests)
            {
                if (guild.CountMember() >= guild.MaxMember())
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                    {
                        message = UITextKeys.UI_ERROR_GUILD_MEMBER_REACHED_LIMIT,
                    });
                }
                // Add guild member to database
                playerCharacter.GuildId = request.guildId;
                UITextKeys createGuildMemberMessage = await CreateGuildMember(validateResult.Guild, SocialCharacterData.Create(playerCharacter), requestHandler.ConnectionId);
                if (createGuildMemberMessage.IsError())
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                    {
                        message = createGuildMemberMessage,
                    });
                    return;
                }

                await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
                {
                    GuildId = validateResult.GuildId,
                    MaxGuildMember = validateResult.Guild.MaxMember(),
                });

                result.Invoke(AckResponseCode.Success, new ResponseSendGuildRequestMessage()
                {
                    message = UITextKeys.UI_GUILD_REQUEST_ACCEPTED,
                });
                return;
            }

            // Add guild request to database
            DatabaseApiResult createRequestResp = await DatabaseClient.CreateGuildRequestAsync(new CreateGuildRequestReq()
            {
                GuildId = request.guildId,
                RequesterId = playerCharacter.Id,
            });
            if (!createRequestResp.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseSendGuildRequestMessage()
            {
                message = UITextKeys.UI_GUILD_REQUESTED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestAcceptGuildRequest(RequestHandlerData requestHandler, RequestAcceptGuildRequestMessage request, RequestProceedResultDelegate<ResponseAcceptGuildRequestMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            // Validate that character can accept other character to join guild or not
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanAcceptGuildRequest(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildRequestMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }

            // Add guild member to database
            if (!GameInstance.ServerUserHandlers.TryGetConnectionId(request.requesterId, out long notifyConnectionId))
                notifyConnectionId = -1;

            // Delete request from database
            DatabaseApiResult deleteRequestResp = await DatabaseClient.DeleteGuildRequestAsync(new DeleteGuildRequestReq()
            {
                GuildId = validateResult.GuildId,
                RequesterId = request.requesterId,
            });
            if (!deleteRequestResp.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            SocialCharacterData requester = default;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.requesterId, out IPlayerCharacterData requesterCharacter))
            {
                requesterCharacter.GuildId = validateResult.GuildId;
                requester = SocialCharacterData.Create(requesterCharacter);
            }
            else
            {
                // No player character in cache, read social character from database
                DatabaseApiResult<SocialCharacterResp> readSocialCharacterResp = await DatabaseClient.GetSocialCharacterAsync(new GetSocialCharacterReq()
                {
                    CharacterId = request.requesterId,
                });
                if (!readSocialCharacterResp.IsSuccess)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildRequestMessage()
                    {
                        message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                    });
                    return;
                }
                requester = readSocialCharacterResp.Response.SocialCharacterData;
                requester.guildId = validateResult.GuildId;
            }

            // Add guild member
            UITextKeys createGuildMemberMessage = await CreateGuildMember(validateResult.Guild, requester, notifyConnectionId);
            if (createGuildMemberMessage.IsError())
            {
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildRequestMessage()
                {
                    message = createGuildMemberMessage,
                });
                return;
            }

            await DatabaseClient.UpdateGuildMemberCountAsync(new UpdateGuildMemberCountReq()
            {
                GuildId = validateResult.GuildId,
                MaxGuildMember = validateResult.Guild.MaxMember(),
            });

            result.Invoke(AckResponseCode.Success, new ResponseAcceptGuildRequestMessage()
            {
                message = UITextKeys.UI_GUILD_REQUEST_ACCEPTED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestDeclineGuildRequest(RequestHandlerData requestHandler, RequestDeclineGuildRequestMessage request, RequestProceedResultDelegate<ResponseDeclineGuildRequestMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            // Validate that character can decline other character to join guild or not
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanDeclineGuildRequest(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildRequestMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }

            // Delete request from database
            DatabaseApiResult deleteRequestResp = await DatabaseClient.DeleteGuildRequestAsync(new DeleteGuildRequestReq()
            {
                GuildId = playerCharacter.GuildId,
                RequesterId = request.requesterId,
            });
            if (!deleteRequestResp.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildRequestMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseDeclineGuildRequestMessage()
            {
                message = UITextKeys.UI_GUILD_REQUEST_DECLINED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestGetGuildRequests(RequestHandlerData requestHandler, RequestGetGuildRequestsMessage request, RequestProceedResultDelegate<ResponseGetGuildRequestsMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetGuildRequestsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            DatabaseApiResult<SocialCharactersResp> getRequestsResp = await DatabaseClient.GetGuildRequestsAsync(new GetGuildRequestsReq()
            {
                GuildId = playerCharacter.GuildId,
                Skip = request.skip,
                Limit = request.limit,
            });
            if (!getRequestsResp.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetGuildRequestsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseGetGuildRequestsMessage()
            {
                guildRequests = getRequestsResp.Response.List,
            });
#endif
        }

        public async UniTaskVoid HandleRequestFindGuilds(RequestHandlerData requestHandler, RequestFindGuildsMessage request, RequestProceedResultDelegate<ResponseFindGuildsMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseFindGuildsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            DatabaseApiResult<GuildsResp> findGuildResp = await DatabaseClient.FindGuildsAsync(new FindGuildNameReq()
            {
                FinderId = playerCharacter.Id,
                GuildName = request.guildName,
                Skip = request.skip,
                Limit = request.limit,
            });
            if (!findGuildResp.IsSuccess)
            {
                result.Invoke(AckResponseCode.Error, new ResponseFindGuildsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseFindGuildsMessage()
            {
                guilds = findGuildResp.Response.List,
            });
#endif
        }

        public UniTaskVoid HandleRequestGetGuildInfo(RequestHandlerData requestHandler, RequestGetGuildInfoMessage request, RequestProceedResultDelegate<ResponseGetGuildInfoMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseGetGuildInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }

            if (!GameInstance.ServerGuildHandlers.TryGetGuild(request.guildId, out GuildData guild))
            {
                result.InvokeError(new ResponseGetGuildInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_GUILD_NOT_FOUND,
                });
                return default;
            }

            result.InvokeSuccess(new ResponseGetGuildInfoMessage()
            {
                guild = new GuildListEntry()
                {
                    Id = guild.id,
                    GuildName = guild.guildName,
                    Level = guild.level,
                    FieldOptions = GuildListFieldOptions.Options,
                    Options = guild.options,
                }
            });
#endif
            return default;
        }

        public async UniTaskVoid HandleRequestGuildRequestNotification(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseGuildRequestNotificationMessage> result)
        {
#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            int notificationCount = 0;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                DatabaseApiResult<GetGuildRequestNotificationResp> resp = await DatabaseClient.GetGuildRequestNotificationAsync(new GetGuildRequestNotificationReq()
                {
                    GuildId = playerCharacter.GuildId,
                });
                if (resp.IsSuccess)
                    notificationCount = resp.Response.NotificationCount;
            }
            result.InvokeSuccess(new ResponseGuildRequestNotificationMessage()
            {
                notificationCount = notificationCount,
            });
#endif
        }
    }
}
