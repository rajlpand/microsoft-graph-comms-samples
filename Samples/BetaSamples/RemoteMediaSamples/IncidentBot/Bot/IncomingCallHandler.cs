// <copyright file="IncomingCallHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace Sample.IncidentBot.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Graph.Communications.Calls;
    using Microsoft.Graph.Communications.Common.Telemetry;
    using Microsoft.Graph.Communications.Resources;
    using Sample.IncidentBot.Data;
    using Sample.IncidentBot.IncidentStatus;

    /// <summary>
    /// The call handler for incoming calls.
    /// </summary>
    public class IncomingCallHandler : CallHandler
    {
        private string endpointId;

        private string callUserObjectID;

        private string userCallID;

        private ICall inboundMeeting;

        private int promptTimes;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncomingCallHandler"/> class.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <param name="call">The call.</param>
        /// <param name="endpointId">The bot endpoint id.</param>
        /// <param name="callUserObjectId">The responder id.</param>
        /// <param name="inboundMeeting">The incident meeting.</param>
        public IncomingCallHandler(Bot bot, ICall call, string endpointId, string callUserObjectId, ICall inboundMeeting)
            : base(bot, call)
        {
            this.endpointId = endpointId;
            this.callUserObjectID = callUserObjectId;
            this.userCallID = call.Id;
            this.inboundMeeting = inboundMeeting;
        }

        /*
        /// <inheritdoc/>
        protected override void CallOnUpdated(ICall sender, ResourceEventArgs<Call> args)
        {
            if (sender.Resource.State == CallState.Established)
            {
                var currentPromptTimes = Interlocked.Increment(ref this.promptTimes);

                if (currentPromptTimes == 1)
                {
                    this.PlayNotificationPrompt();
                }
            }
        }

        /// <summary>
        /// Play the notification prompt.
        /// </summary>
        private void PlayNotificationPrompt()
        {
            Task.Run(async () =>
            {
                try
                {
                    var mediaName = this.endpointId == null ? Bot.BotIncomingPromptName : Bot.BotEndpointIncomingPromptName;

                    await this.Call.PlayPromptAsync(new List<MediaPrompt> { this.Bot.MediaMap[mediaName] }).ConfigureAwait(false);
                    this.Logger.Info("Started playing notification prompt");
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"Failed to play notification prompt.");
                    throw;
                }
            });
        }
    */

        /// <inheritdoc/>
        protected override void CallOnUpdated(ICall sender, ResourceEventArgs<Call> args)
        {
            // this.statusData?.UpdateResponderNotificationStatus(this.responderId, sender.Resource.State);
            if (sender.Resource.State == CallState.Established)
            {
                var currentPromptTimes = Interlocked.Increment(ref this.promptTimes);

                if (currentPromptTimes == 1)
                {
                    this.SubscribeToTone();
                    this.PlayNotificationPrompt();
                }

                if (sender.Resource.ToneInfo?.Tone != null)
                {
                    Tone tone = sender.Resource.ToneInfo.Tone.Value;

                    this.Logger.Info($"Tone {tone} received.");

                    // handle different tones from responder
                    switch (tone)
                    {
                        case Tone.Tone1:
                            this.PlayTransferingPrompt();
                            this.TransferToIncidentMeeting();
                            break;
                        case Tone.Tone0:
                        default:
                            this.PlayNotificationPrompt();
                            break;
                    }

                    sender.Resource.ToneInfo.Tone = null;
                }
            }
        }

        /// <summary>
        /// Subscribe to tone.
        /// </summary>
        private void SubscribeToTone()
        {
            Task.Run(async () =>
            {
                try
                {
                    await this.Call.SubscribeToToneAsync().ConfigureAwait(false);
                    this.Logger.Info("IncomingCallHandler: Started subscribing to tone.");
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"IncomingCallHandler: Failed to subscribe to tone.");
                    throw;
                }
            });
        }

        /// <summary>
        /// Play the transfering prompt.
        /// </summary>
        private void PlayTransferingPrompt()
        {
            Task.Run(async () =>
            {
                try
                {
                    await this.Call.PlayPromptAsync(new List<MediaPrompt> { this.Bot.MediaMap[Bot.TransferingPromptName] }).ConfigureAwait(false);
                    this.Logger.Info("IncomingCallHandler: Started playing transfering prompt");
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"IncomingCallHandler: Failed to play transfering prompt.");
                    throw;
                }
            });
        }

        /// <summary>
        /// Play the notification prompt.
        /// </summary>
        private void PlayNotificationPrompt()
        {
            Task.Run(async () =>
            {
                try
                {
                    var mediaName = this.endpointId == null ? Bot.BotIncomingPromptName : Bot.BotEndpointIncomingPromptName;
                    await this.Call.PlayPromptAsync(new List<MediaPrompt> { this.Bot.MediaMap[mediaName] }).ConfigureAwait(false);
                    this.Logger.Info("IncomingCallHandler: Started playing notification prompt");
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"IncomingCallHandler: Failed to play notification prompt.");
                    throw;
                }
            });
        }

        /// <summary>
        /// add current responder to incident meeting as participant.
        /// </summary>
        private void TransferToIncidentMeeting()
        {
            Task.Run(async () =>
            {
                try
                {
                    this.Logger.Info($"IncomingCallHandler: Starting transfer to incident meeting. Current call ID {this.userCallID}, User Object ID {this.callUserObjectID} and Bot Meeting ID {this.inboundMeeting.Id}.");
                    if (this.userCallID != null && this.callUserObjectID != null)
                    {
                        var addParticipantRequestData = new AddParticipantRequestData()
                        {
                            ObjectId = this.callUserObjectID,
                            ReplacesCallId = this.userCallID,
                        };

                        await this.Bot.AddParticipantAsync(this.inboundMeeting.Id, addParticipantRequestData).ConfigureAwait(false);

                        this.Logger.Info("IncomingCallHandler: Finished to transfer to incident meeting. ");
                    }
                    else
                    {
                        this.Logger.Warn(
                            $"IncomingCallHandler: Tried to transfer to incident meeting but needed info are not valid. Meeting call-id: {this.inboundMeeting.Id};");
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"IncomingCallHandler: Failed to transfer to incident meeting.");
                    throw;
                }
            });
        }
    }
}
