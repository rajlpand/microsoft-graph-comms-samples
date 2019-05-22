// <copyright file="IncidentRequestData.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace Sample.IncidentBot.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The incident request data.
    /// </summary>
    public class IncidentRequestData : JoinCallRequestData
    {
        /// <summary>
        /// Gets or sets the incident name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the incident time.
        /// </summary>
        public string ObjectId1 { get; set; }

        /// <summary>
        /// Gets or sets the user object ids.
        /// </summary>
        public string ObjectId2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the responders are applications or users.
        /// Value can be Application (For testing purpose) or User.
        /// By default is User.
        /// </summary>
        public string ResponderType { get; set; }
    }
}
