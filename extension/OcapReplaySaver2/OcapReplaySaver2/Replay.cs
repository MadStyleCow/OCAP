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
            this.Markers = new List<object>();
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
        /// This method is called, once a vehicle's position is updated.
        /// </summary>
        /// <param name="pArguments">An array of arguments</param>
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
        /// This method is called, once a marker is created.
        /// </summary>
        /// <param name="pArguments">An array of arguments</param>
        public void onRegisterMarker(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 11)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // First, we need to check the color of the marker
            string _color = pArguments[7].Replace("\"", "");

            if (_color.Equals("any"))
            {
                // Set the color to 000000
                pArguments[0] = "000000";
            }

            // Create a list of marker parameters
            List<object> _marker = new List<object>();

            // Add parameters
            _marker.Add(pArguments[0].Replace("\"", ""));
            _marker.Add(int.Parse(pArguments[1]));
            _marker.Add(pArguments[2].Replace("\"", ""));
            _marker.Add(pArguments[3].Replace("\"", ""));
            _marker.Add(int.Parse(pArguments[4]));
            _marker.Add(int.Parse(pArguments[5]));
            _marker.Add(int.Parse(pArguments[6]));
            _marker.Add(_color);
            _marker.Add(JsonConvert.DeserializeObject(pArguments[8]));
            _marker.Add(int.Parse(pArguments[9]));
            _marker.Add(new List<object[]>()
            {
                new object[]
                {
                    int.Parse(pArguments[4]),
                    JsonConvert.DeserializeObject(pArguments[10]),  // This 
                    int.Parse(pArguments[1])    // And this should actually be an object
                }
            });

            // Push the result into the marker list
            this.Markers.Add(_marker.ToArray());
        }

        /// <summary>
        /// This method is called, once a marker position has been updated.
        /// </summary>
        /// <param name="pArguments">An array of arguments</param>
        public  void onUpdateMarker(string[] pArguments)
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

            // Does the marker exist?
            if (this.Markers.Exists(x => (string)(((object[])x)[0]) == (pArguments[0].Replace("\"", ""))))
            {
                // Get the marker
                object[] _marker = (object[]) this.Markers.Find(x => (string)(((object[])x)[0]) == (pArguments[0].Replace("\"", "")));

                // Get the array of marker frames
                List<object[]> _markerFrames = (List<object[]>) _marker[10];

                // Which frame are we looking for?
                var _targetFrame = int.Parse(pArguments[1]);

                // Does a frame entry exist for this marker?
                if (_markerFrames.Exists(x => (int)x[0] == _targetFrame))
                {
                    // Such a marker frame exists, update it
                    object[] _markerFrame = _markerFrames.Find(x => (int)x[0] == _targetFrame);

                    // Update it.
                    _markerFrame[0] = _targetFrame;
                    _markerFrame[1] = JsonConvert.DeserializeObject(pArguments[2]);
                    _markerFrame[2] = 0;
                }
                else
                {
                    // One does not exist.
                    // Push a new entry
                    _markerFrames.Add(new object[]
                    {
                        _targetFrame,
                        JsonConvert.DeserializeObject(pArguments[2]),
                        0
                    });
                }
            }
        }

        /// <summary>
        /// This method is called, once a marker has been removed.
        /// </summary>
        /// <param name="pArguments">An array of arguments.</param>
        public void onRemoveMarker(string[] pArguments)
        {
            // Check amount of arguments
            if (pArguments.Length != 2)
            {
                throw new ArgumentException("Incorrect amount of arguments");
            }

            // Should we record this?
            if (!_shouldRecord)
            {
                return;
            }

            // Does the marker exist?
            if (this.Markers.Exists(x => (string)(((object[])x)[0]) == (pArguments[0].Replace("\"", ""))))
            {
                // Get the marker
                var _marker = this.Markers.Find(x => (string)(((object[])x)[0]) == (pArguments[0].Replace("\"", "")));

                // Set the property (I assume its deletedAtFrame)
                ((object[])_marker)[5] = int.Parse(pArguments[1]);
            }
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

            // Discard any previous data.
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

            this.prepareMarkerFrames(this.TotalFrames);

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
            this.Markers.Clear();

            // Indicate we are no longer writing
            this._shouldRecord = false;
        }
        #endregion

        /* Private methods */
        private void prepareMarkerFrames(int pTotalFrames)
        {
            for (int i = 0; i < this.Markers.Count; i++)
            {
                object[] _marker = (object[]) this.Markers[i];

                // Create a replacement object array
                object[] _replacementArray = new object[]
                {
                    _marker[2],
                    _marker[3],
                    _marker[4],
                    (int) _marker[5] == -1 ? pTotalFrames : _marker[5],
                    _marker[6],
                    _marker[7],
                    _marker[9],
                    _marker[10]
                };

                // Replace the markers
                this.Markers[i] = _replacementArray;
            }
        }
        

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
                        InsertURL = "http://changeme.com",
                        CopyLocal = true,
                        FileDestination = @"C:\",
                        Timeout = 120
                    }));
            }

            // Read the config
            Config _config = (Config)JsonConvert.DeserializeObject(File.ReadAllText("./config/wog_ocap.json"), typeof(Config));

            // Define a file name
            var _fileName = String.Format("{0}_{1}_{2}.json", 
                DateTime.Now.ToString("yyyy_MM_dd__hh_mm"),
                this.Mission, this.World);

            // Do we write local or write remote?
            if (_config.CopyLocal)
            {
                // Write the JSON to the file
                File.WriteAllText(Path.Combine(_config.FileDestination, _fileName),
                    JsonConvert.SerializeObject(this));
            }
            else
            {
                // TODO: Implement remote server upload
            }

            // TODO: Extract requests into separate methods / class
            // Insert a record into the database
            // Create a URL string
            var _dbUrlBuilder = new StringBuilder();

            _dbUrlBuilder.Append(_config.InsertURL);
            _dbUrlBuilder.Append(String.Format("&worldName={0}", Uri.EscapeDataString(this.World)));
            _dbUrlBuilder.Append(String.Format("&missionName={0}", Uri.EscapeDataString(this.Mission)));
            _dbUrlBuilder.Append(String.Format("&missionDuration={0}", Uri.EscapeDataString(this.TotalFrames.ToString())));
            _dbUrlBuilder.Append(String.Format("&filename={0}", Uri.EscapeDataString(_fileName)));
            _dbUrlBuilder.Append("&type=wog");

            // Now we need to make an HTTP request to add the entry to the database.
            // Make the actual response
            HttpWebRequest _dbRequest = (HttpWebRequest)WebRequest.Create(_dbUrlBuilder.ToString());

            // Set parameters
            _dbRequest.Method = WebRequestMethods.Http.Get;
            _dbRequest.Timeout = _config.Timeout * 1000;

            // Make the request
            _dbRequest.GetResponse();
        }
    }
}
