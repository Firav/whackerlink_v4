/*
 * Copyright (C) 2024-2025 Caleb H. (K4PHP) caleb.k4php@gmail.com
 *
 * This file is part of the WhackerLinkServer project.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 *
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 */

using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using WhackerLinkLib.Models;
using WhackerLinkLib.Models.IOSP;

namespace WhackerLinkServer
{
    /// <summary>
    /// Class to send HTTP POST reports 
    /// </summary>
    public class Reporter
    {
        private string address;
        private int port;
        private ILogger logger;
        private bool enabled;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates an instance of Reporter
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        public Reporter(string address, int port, ILogger logger, bool enabled = false)
        {
            this.address = address;
            this.port = port;
            this.enabled = enabled;
            this.logger = logger;

            if (!enabled)
                return;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{address}:{port}")
            };

            logger.Information("Started Reporter at http://{Address}:{Port}", address, port);
        }

        /// <summary>
        /// Create and send a report async
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public async Task SendReportAsync(object reportData)
        {
            if (!enabled)
                return;

            var json = JsonConvert.SerializeObject(reportData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
                
            try
            {
                var response = await _httpClient.PostAsync("/", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUG
                    logger.Information("[REPORTER] Report sent");
#endif
                }
                else
                {
                    logger.Error($"[REPORTER] Failed to send: {response.StatusCode}");
                }
                }
            catch (Exception ex)
            {
                logger.Error($"[REPORTER] Error sending report: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper function to send a report
        /// </summary>
        /// <param name="type"></param>
        /// <param name="srcId"></param>
        /// <param name="dstId"></param>
        /// <param name="extra"></param>
        public void Send(PacketType type, string srcId, string dstId, Site site, string extra, ResponseType responseType = ResponseType.UNKOWN, string lat = null, string longi = null)
        {
            if (!enabled)
                return;

            var utcNow = DateTime.UtcNow;

            TimeZoneInfo cdtZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cdtTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, cdtZone);

            string timestamp = cdtTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);

            var reportData = new
            {
                Type = type,
                SrcId = srcId,
                DstId = dstId,
                Site = site,
                ResponseType = responseType,
                Extra = extra,
                Lat = lat,
                Long = longi,
                Timestamp = timestamp
            };

            Task.Run(() => SendReportAsync(reportData));
        }

        /// <summary>
        /// Helper to send site bcast report
        /// </summary>
        /// <param name="type"></param>
        ///
        public void Send(PacketType type, SITE_BCAST siteBcast)
        {
            if (!enabled)
                return;

            var utcNow = DateTime.UtcNow;

            TimeZoneInfo cdtZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cdtTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, cdtZone);

            string timestamp = cdtTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);

            var reportData = new
            {
                Type = type,
                Sites = siteBcast.Sites,
                Timestamp = timestamp
            };

            Task.Run(() => SendReportAsync(reportData));
        }

        /// <summary>
        /// Helper to send sts bcast report
        /// </summary>
        /// <param name="type"></param>
        ///
        public void Send(PacketType type, STS_BCAST stsBcast)
        {
            if (!enabled)
                return;

            var utcNow = DateTime.UtcNow;

            TimeZoneInfo cdtZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cdtTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, cdtZone);

            string timestamp = cdtTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);

            var reportData = new
            {
                Type = type,
                Site = stsBcast.Site,
                Status = stsBcast.Status,
                Timestamp = timestamp
            };

            Task.Run(() => SendReportAsync(reportData));
        }
    }
}
