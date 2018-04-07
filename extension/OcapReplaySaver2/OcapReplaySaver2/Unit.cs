using Newtonsoft.Json;
using System.Collections.Generic;

namespace OcapReplaySaver2
{
    public class Unit
    {
        /* Public properties */
        #region Public properties
        
        /// <summary>
        /// ID of the unit
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }

        /// <summary>
        /// Name of the unit
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Name of the unit
        /// </summary>
        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }

        /// <summary>
        /// Group name (e.g. Alpha-1-1) of the unit
        /// </summary>
        [JsonProperty(PropertyName = "group")]
        public string GroupName { get; set; }

        /// <summary>
        /// Side of the unit (EAST, WEST, GUER, CIV)
        /// </summary>
        [JsonProperty(PropertyName = "side")]
        public string Side { get; set; }

        /// <summary>
        /// Is this a player controller unit
        /// </summary>
        [JsonProperty(PropertyName = "isPlayer")]
        public bool IsPlayer { get; set; }

        /// <summary>
        /// Type of the unit (?)
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// First seen at frame
        /// </summary>
        [JsonProperty(PropertyName = "startFrameNum")]
        public int StartFrame { get; set; }

        /// <summary>
        /// A list of all the positions, the unit takes
        /// </summary>
        [JsonProperty(PropertyName = "positions")]
        public List<object> Positions { get; set; }

        /// <summary>
        /// A list of the frames, when the unit was firing (and the direction)
        /// </summary>
        [JsonProperty(PropertyName = "framesFired")]
        public List<object> Fired { get; set; }
        #endregion
    }
}
