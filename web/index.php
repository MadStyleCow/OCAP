<?php
include "common.php";
$dbPath = "data/data.db";

// Check if database exists
if (!file_exists($dbPath)) {
	echo "
	<div style='font-family: Arial, sans-serif;'>
		<b>Error:</b> Database not found. Please ensure you have ran the <a href='install/'>installer</a> first.
	</div>
	";
	exit();
};

// Get server's id from local DB
$db = new PDO('sqlite:' . $dbPath);
$result = $db->query("SELECT remote_id FROM servers");
$row = $result->fetchObject();
$id = $row->remote_id; // Used later when contacting stats server
print_debug("ID: " . $id);

// Increment view count in local DB
$db->exec(sprintf("UPDATE servers SET view_count = view_count + 1 WHERE remote_id = %d", $id)); 

// Close DB
$db = NULL;

// Contact stats server to increment view count
// Please do not modify this as these stats help me get a job. Thank-you! :)
$result = curlRemote("stats-manager.php", array(
	"option" => "increment_view_count",
	"server_id" => $id
));
print_debug("Result: " . $result);

// Check remote server for any urgent messages
$result = curlRemote("stats-manager.php", array(
	"option" => "get_urgent_message",
	"version" => VERSION
));

if ($result != "") {
	echo sprintf("<script>alert(\"%s\")</script>", $result);
	print_debug($result);
}
?>
<!DOCTYPE html>

<html>
	<head>
		<!-- Title -->
		<title><?php echo $appTitle; ?></title>

		<!-- Meta -->
		<meta charset = "utf-8"/>
		<meta name = "viewport" content = "initial-scale=1.0, user-scalable=no" />

		<!-- Stylesheets -->
		<link rel = "stylesheet" href = "leaflet/leaflet.css" />
		<link rel = "stylesheet" href = "style/common.css" />
		<link rel = "stylesheet" href = "style/index.css" />

		<!-- Favicon -->
		<link rel = "icon" type = "img/png" href = "images/favicon.png" />
	</head>

	<!-- Body -->
	<body>
		<div id = "container">
			<!-- Map -->
			<div id = "map"></div>

			<!-- Top menu panel -->
			<div id = "topPanel">
				<!-- Share buttons (share the URL to the specific replay) -->
				<div id = "shareButton" class = "bold button"></div>

				<!-- Info button -->
				<div id = "aboutButton" class = "bold button">i</div>

				<!-- Mission name -->
				<span id = "missionName" class = "medium"></span>
			</div>

			<!-- Let panel -->
			<div id = "leftPanel">
				<div id = "controlSide" style = "display:inline-block">
					<div id = "sideWest" class = "blufor sideTitle" style = "">BLUFOR</div>
					<div id = "sideEast" class = "opfor sideTitle">OPFOR</div>
					<div id = "sideGuer" class = "ind sideTitle">IND</div>
					<div id = "sideCiv" class = "civ sideTitle">CIV</div>
				</div>
				<div class = "panelContent">
					<ul id = "listWest"></ul>
					<ul id = "listEast"></ul>
					<ul id = "listGuer"></ul>
					<ul id = "listCiv"></ul>
				</div>
			</div>

			<!-- Right panel -->
			<div id = "rightPanel">
				<div class = "title bold">События</div>
				<div class = "filterBox">
					<div id = "filterHitEventsButton" class = "filterHit"></div>
					<div id = "filterConnectEventsButton" class = "filterConnect"></div>
					<input type = "text" id = "filterEventsInput" placeholder = "Фильтр" />
				</div>
				<div class = "panelContent">
					<ul id = "eventList"></ul>
				</div>
			</div>

			<!-- Info box -->
			<div class = "extraInfoBox">
				<div class = "extraInfoBoxContent">
					<span class = "bold">Cursor target: </span><span id = "cursorTargetBox">None</span>
				</div>
			</div>

			<!-- Bototm panel -->
			<div id = "bottomPanel">
				<div id = "frameSliderContainer">
						<input type = "range" id = "frameSlider" min = "0" value = "0">
							<div id = "eventTimeline"></div>
						</input>
				</div>
				<div class = "panelContent">
					<div id = "playPauseButton" onclick = "playPause()">
					</div>
					<div id = "timecodeContainer" class = "medium">
						<span id = "missionCurTime">0:00:00</span>
						<span>/</span>
						<span id = "missionEndTime">0:00:00</span>
					</div>
					<div class = "fullscreenButton" onclick = "goFullscreen()"></div>
					<div id = "playbackSpeedSliderContainer">
						<span id = "playbackSpeedVal">10x</span>
						<input type = "range" id = "playbackSpeedSlider" />
					</div>
					<span id = "toggleFirelines"></span>
					<span id = "toggleNickname"></span>
					<span id = "toggleMapMarker"></span>
					
				</div>
			</div>
		</div>

		<!-- Replay selection modal -->
		<div id = "modal" class = "modal">
			<div class = "modalContent">
				<div id = "modalHeader" class = "modalHeader medium">Header</div>
				<div id = "modalFilter" class = "modalFilter">
					<input type = "text" id = "filterGameInput" placeholder = "Название миссии" />
					<input type = "date" id = "calendar1" value = "2017-06-01">
					<input type = "date" id = "calendar2" value=<?php echo(date("Y-m-d"))?>>
					<div id = "filterSubmit"></div>
				</div>
				<div id = "modalBody" class = "modalBody">Body</div>
				<div id = "modalButtons" class = "modalButtons"></div>
			</div>
		</div>

		<!-- Hint -->
		<div id="hint" class="hint">Test popup</div>

		<!-- Scripts -->
		<script src = "leaflet/leaflet.js"></script>
		<script src = "leaflet/leaflet.rotatedMarker.js"></script>
		<script src = "leaflet/leaflet.svgIcon.js"></script>
		<script src = "scripts/jquery.min.js"></script>
		<script src = "scripts/ocap.entity.js"></script>
		<script src = "scripts/ocap.event.js"></script>
		<script src = "scripts/ocap.group.js"></script>
		<script src = "scripts/ocap.groups.js"></script>
		<script src = "scripts/ocap.vehicle.js"></script>
		<script src = "scripts/ocap.unit.js"></script>
		<script src = "scripts/ocap.ui.js"></script>
		<script src = "scripts/ocap.js"></script>
	</body>

	<!-- Init after parsing has complete -->
	<script>
		// Define some block level variables
		let appVersion = <?php echo json_encode(VERSION); ?>;
		let appTitle = <?php echo json_encode($appTitle); ?>;
		let appDesc = <?php echo json_encode($appDesc); ?>;
		let appAuthor = <?php echo json_encode($appAuthor); ?>;

		// Init ocap
		initOCAP();
	</script>
</html>