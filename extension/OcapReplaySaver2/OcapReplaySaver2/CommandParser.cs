using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OcapReplaySaver2
{
    /// <summary>
    /// A static class for command parsing
    /// </summary>
    public static class CommandParser
    {
        /// <summary>
        /// A static method, called to parse arguments.
        /// </summary>
        /// <param name="pCommand">A string representing a command.</param>
        /// <param name="pArguments">An array of command arguments (data)</param>
        public static void ParseCommand(string pCommand, string[] pArguments) {
            // Once this is called, we need to figure out what to do next.
            // We have a map between the commands and functions those commands execute.
            // So look up what we need to do and call it.

            switch(pCommand)
            {
                case ":NEW:UNIT:":
                    Replay.Instance.onRegisterUnit(pArguments);
                    break;

                case ":UPDATE:UNIT:":
                    Replay.Instance.onUpdateUnit(pArguments);
                    break;

                case ":NEW:VEH:":
                    Replay.Instance.onRegisterVehicle(pArguments);
                    break;

                case ":UPDATE:VEH:":
                    Replay.Instance.onUpdateVehicle(pArguments);
                    break;

                case ":MARKER:CREATE:":
                    Replay.Instance.onRegisterMarker(pArguments);
                    break;

                case ":MARKER:MOVE:":
                    Replay.Instance.onUpdateMarker(pArguments);
                    break;

                case ":MARKER:DELETE:":
                    Replay.Instance.onRemoveMarker(pArguments);
                    break;

                case ":EVENT:":
                    Replay.Instance.onEvent(pArguments);
                    break;

                case ":FIRED:":
                    Replay.Instance.onEntityFired(pArguments);
                    break;

                case ":START:":
                    Replay.Instance.onMissionStarted(pArguments);
                    break;

                case ":SAVE:":
                    Replay.Instance.onMissionEnded(pArguments);
                    break;

                case ":CLEAR:":
                    Replay.Instance.onClear(pArguments);
                    break;

            }
        }
    }
}
