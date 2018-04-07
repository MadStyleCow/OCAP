using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OcapReplaySaver2
{
    /// <summary>
    /// A singleton class, reponsible for storing data and providing methods for interaction with the data
    /// </summary>
    public class Replay
    {
        /* Initialization */
        #region Initialization
        // Define singleton reference
        private static readonly Replay _instance = new Replay();

        // Define an explicit static constructor
        static Replay() { }

        // Define a proper constructor
        private Replay() {
            // Create the lists
            this.Units = new List<Unit>();
            this.Events = new List<object>();
        }
        #endregion

        /* Private variables */
        #region Private variables
        /// <summary>
        /// Indicates, whether the event handlers should be active
        /// </summary>
        private bool _shouldRecord = false;
        #endregion

        /* System properties */
        #region System properties
        /// <summary>
        /// An instance of the class
        /// </summary>
        [JsonIgnore]
        public static Replay Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        /* Public properties */
        #region Public properties
        /// <summary>
        /// Name of the current island (world)
        /// </summary>
        [JsonProperty(PropertyName = "worldName")]
        public string World { get; set; }

        /// <summary>
        /// Name of the current mission
        /// </summary>
        [JsonProperty(PropertyName = "missionName")]
        public string Mission { get; set; }

        /// <summary>
        /// Author of the current mission
        /// </summary>
        [JsonProperty(PropertyName = "missionAuthor")]
        public string Author { get; set; }

        /// <summary>
        /// Capture delay in frames (as in how many frames pass, before a new capture is taken)
        /// </summary>
        [JsonProperty(PropertyName = "captureDelay")]
        public int Delay { get; set; }

        /// <summary>
        /// A number indicating the end frame
        /// </summary>
        [JsonProperty(PropertyName = "endFrame")]
        public int TotalFrames { get; set; }

        /// <summary>
        /// A list of all entities on the map
        /// </summary>
        [JsonProperty(PropertyName = "entities")]
        public List<Unit> Units { get; set; }

        /// <summary>
        /// A list of all events
        /// </summary>
        [JsonProperty(PropertyName = "events")]
        public List<object> Events { get; set; }

        /// <summary>
        /// A list of all markers
        /// </summary>
        [JsonProperty(PropertyName = "Markers")]
        public List<object> Markers { get; set; }
        #endregion

        /* Public methods */
        #region Public methods
        /// <summary>
        /// This method is called, once a new unit should be registered in the unit list
        /// </summary>
        /// <param name="pArguments">An array of arguments</param>
        public void onRegisterUnit(string[] pArguments)
        {
            // Check the amount of available arguments
            if (pArguments.Length != 6)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Construct a new unit entry
            this.Units.Add(new Unit()
            {
                Type = "unit",
                StartFrame = int.Parse(pArguments[0]),
                ID = int.Parse(pArguments[1]),
                Name = pArguments[2].Replace("\"", ""),
                GroupName = pArguments[3].Replace("\"", ""),
                Side = pArguments[4].Replace("\"", ""),
                IsPlayer = Convert.ToBoolean(Convert.ToInt16(pArguments[5])),
                Fired = new List<object>(),
                Positions = new List<object>()
            });
        }

        /// <summary>
        /// This method is called, once a new vehicle should be registered in the unit list
        /// </summary>
        /// <param name="pArguments">An array of arguments</param>
        public void onRegisterVehicle(string[] pArguments)
        {
            // Check the amount of available arguments
            if (pArguments.Length != 4)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Construct a new unit entry
            this.Units.Add(new Unit()
            {
                Type = "vehicle",
                StartFrame = int.Parse(pArguments[0]),
                ID = int.Parse(pArguments[1]),
                Class = pArguments[2].Replace("\"", ""),
                Name = pArguments[3].Replace("\"", ""),
                Fired = new List<object>(),
                Positions = new List<object>()
            });
        }

        /// <summary>
        /// This method is called, once an event happens.
        /// </summary>
        /// <param name="pArguments">An array of event arguments</param>
        public void onEvent(string[] pArguments)
        {

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Create a new object list
            List<object> _tempList = new List<object>();

            // Update the mission length, based on the received data
            if (this.TotalFrames < int.Parse(pArguments[0]))
            {
                this.TotalFrames = int.Parse(pArguments[0]);
            }

            // Since there are two separate cases of events, handle them differently
            switch (pArguments.Length)
            {
                case 3:
                    _tempList.Add(int.Parse(pArguments[0]));
                    _tempList.Add(pArguments[1].Replace("\"", ""));
                    _tempList.Add(pArguments[2].Replace("\"", ""));
                    break;

                case 5:
                    _tempList.Add(int.Parse(pArguments[0]));
                    _tempList.Add(pArguments[1].Replace("\"", ""));
                    _tempList.Add(int.Parse(pArguments[2]));
                    _tempList.Add(JsonConvert.DeserializeObject(pArguments[3]));
                    _tempList.Add(int.Parse(pArguments[4]));
                    break;

                default:
                    return;
            }

            // Push a new event into the event array
            this.Events.Add(_tempList.ToArray());
        }

        /// <summary>
        /// This method is called, once a units position is updated.
        /// </summary>
        /// <param name="pArguments">An array of position arguments</param>
        public void onUpdateUnit(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 7)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Does a unit with this ID exist?
            if (this.Units.Exists(x => x.ID == int.Parse(pArguments[0]))) {
                // Yes it does
                Unit _unit = this.Units.Find(x => x.ID == int.Parse(pArguments[0]));

                // But we need to push less data to it, than is in the array we have
                _unit.Positions.Add(
                    new object[] {
                        JsonConvert.DeserializeObject(pArguments[1]),
                        int.Parse(pArguments[2]),
                        int.Parse(pArguments[3]),
                        int.Parse(pArguments[4]),
                        pArguments[5].Replace("\"", ""),
                        int.Parse(pArguments[6])
                });

                // Update the total time of the mission
                if (TotalFrames < _unit.Positions.Count)
                {
                    TotalFrames = _unit.Positions.Count;
                }
            }
        }

        public void onUpdateVehicle(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 5)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Does a unit with this ID exist?
            if (this.Units.Exists(x => x.ID == int.Parse(pArguments[0])))
            {
                // Yes it does
                Unit _unit = this.Units.Find(x => x.ID == int.Parse(pArguments[0]));

                // But we need to push less data to it, than is in the array we have
                _unit.Positions.Add(
                    new object[] {
                        JsonConvert.DeserializeObject(pArguments[1]),
                        int.Parse(pArguments[2]),
                        int.Parse(pArguments[3]),
                        JsonConvert.DeserializeObject(pArguments[4]),
                });

                // Update the total time of the mission
                if (TotalFrames < _unit.Positions.Count)
                {
                    TotalFrames = _unit.Positions.Count;
                }
            }
        }

        /// <summary>
        /// This method is called, once a mission has been started.
        /// </summary>
        /// <param name="pArguments">Array of arguments describing a mission.</param>
        public void onMissionStarted(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 4)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // At this point, we need to check. Were we previously recording?
            if (_shouldRecord)
            {
                // Yes we were. Dump whatever we had on to the disk.
                this.Export();
            }
            
            // In any case, we should clear all the data.
            this.onClear(pArguments);

            // Indicate that we are now recording.
            _shouldRecord = true;

            // Initialize a new instance of the replay
            this.World = pArguments[0].Replace("\"", "");
            this.Mission = pArguments[1].Replace("\"", "");
            this.Author = pArguments[2].Replace("\"", "");
            this.Delay = int.Parse(pArguments[3]);
        }

        /// <summary>
        /// This method is called, once an entity fires.
        /// </summary>
        /// <param name="pArguments">Array of arguments</param>
        public void onEntityFired(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 3)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Does a unit with this ID exist?
            if (this.Units.Exists(x => x.ID == int.Parse(pArguments[0])))
            {
                // Yes it does
                Unit _unit = this.Units.Find(x => x.ID == int.Parse(pArguments[0]));

                // But we need to push less data to it, than is in the array we have
                _unit.Fired.Add(
                    new object[] {
                        int.Parse(pArguments[1]),
                        JsonConvert.DeserializeObject(pArguments[2])
                });
            }
        }

        /// <summary>
        /// This method is called, once a missions ends.
        /// </summary>
        /// <param name="pArguments"></param>
        public void onMissionEnded(string[] pArguments, bool pIgnoreArguments = false)
        {
            // Check amount of arguments
            if (!pIgnoreArguments && pArguments.Length != 5)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Set the total length
            // No need to set everything else, as it will probably be already set.
            if (pArguments.Length > 0)
            {
                this.World = pArguments[0].Replace("\"", "");
                this.Mission = pArguments[1].Replace("\"", "");
                this.Author = pArguments[2].Replace("\"", "");
                this.Delay = int.Parse(pArguments[3]);
                this.TotalFrames = int.Parse(pArguments[4]);
            }

            // Now, we need to parse the output into a JSON file and export it.
            this.Export();

            // And then clear to get rid of the data
            // (even though a clear will happen anyways once a new mission is started)
            this.onClear(pArguments);
        }

        /// <summary>
        /// This is method is called to reset the replay object.
        /// </summary>
        /// <param name="pArguments"></param>
        public void onClear(string[] pArguments)
        {
            // Clear all of the existing data
            this.World = "";
            this.Mission = "";
            this.Author = "";
            this.Delay = 1;
            this.TotalFrames = 0;

            // Clear all existing lists
            this.Units.Clear();
            this.Events.Clear();
            this.Markers = null;
        }
        #endregion

        /* Private methods */
        /// <summary>
        /// This method is called to export the data into a JSON file.
        /// </summary>
        private void Export()
        {
            // Now, this was called, so we need to export to an external file and then, make the respective web-calls.
            // Get the config file from the disk.
            if (!File.Exists("./config/wog_ocap.json"))
            {
                // Create one and place default parameters
                File.WriteAllText("./config/wog_ocap.json",
                    JsonConvert.SerializeObject(new Config()
                    {
                        AddURL = "http://changeme.com",
                        FileDestination = @"C:\",
                        InsertURL = "http://changeme.com",
                        Timeout = 120
                    }));
            }

            // Read the config
            Config _config = (Config)JsonConvert.DeserializeObject(File.ReadAllText("./config/wog_ocap.json"), typeof(Config));

            // Define a file name
            var _fileName = String.Format("{0}.json", Path.GetRandomFileName());

            // Write the JSON to the file
            File.WriteAllText(Path.Combine(_config.FileDestination, _fileName),
                JsonConvert.SerializeObject(this));

            // Create a URL string
            var _urlBuilder = new StringBuilder();

            _urlBuilder.Append(_config.InsertURL);
            
            _urlBuilder.Append(String.Format("&worldName={0}", Uri.EscapeDataString(this.World)));
            _urlBuilder.Append(String.Format("&missionName={0}", Uri.EscapeDataString(this.Mission)));
            _urlBuilder.Append(String.Format("&missionDuration={0}", Uri.EscapeDataString(this.TotalFrames.ToString())));
            _urlBuilder.Append(String.Format("&filename={0}", _fileName));
            _urlBuilder.Append("&type=wog");

            // Now we need to make an HTTP request to add the entry to the database.
            // Make the actual response
            HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(_urlBuilder.ToString());

            // Set parameters
            _request.Method = WebRequestMethods.Http.Get;
            _request.Timeout = _config.Timeout;

            // Make the request
            _request.GetResponseAsync();
        }
    }
}
