<?php

include "../common.php";

if (count($_GET) == 0) {
	echo "No data received.";
	exit;
}

if (!in_array($_SERVER['REMOTE_ADDR'], $ipGameServer)) die();

if (!file_exists("data.db")) {
	echo "Database not found. Please ensure you have ran the installer first.";
	exit;
}

$option = $_GET["option"];

//echo "Raw data received: " .implode(" ", $_GET) . "\n";
$string = "";
foreach($_GET as $key => $value) {
	$string .= $key . ": " . $value . "\n";
}

// Truncate string if too large
if (strlen($string) > 500) {
	$string = substr($string, 0, 500) . "...";
}
echo "Processed data: \n". $string . "\n";

if ($option == "addFile") { // Add receieved file to this directory
	$fileName = urldecode($_POST["fileName"]);
	$fileContents = $_FILES["fileContents"];

	try {
		if (move_uploaded_file($fileContents["tmp_name"],$fileName )) {
			echo "Successfully created file.";
		} else {
			echo "No successfully created file.";
		};
	} catch (Exception $e) {
		echo $e->getMessage();
	}
	$data = implode("", file($fileName));
	$gzdata = gzencode($data, 9);
	$fp = fopen("$fileName.gz", "w");
	fwrite($fp, $gzdata);
	fclose($fp);
} elseif ($option == "dbInsert") { // Insert values into SQLite database
	$date = date("Y-m-d");
	$serverId = -1;
	try {
		$db = new PDO('sqlite:data.db');

		// Add operation to database
		$db->exec(sprintf("
			INSERT INTO operations (world_name, mission_name, mission_duration, filename, date, type) VALUES (
				'%s',
				'%s',
				%d,
				'%s',
				'%s',
				'%s'
			)
		", $_GET["worldName"], $_GET["missionName"], $_GET["missionDuration"], $_GET["filename"], $date, $_GET["type"]));

		// TODO: Increment local capture count

		// Get server ID
		$results = $db->query("SELECT remote_id FROM servers");
		$serverId = $results->fetch()["remote_id"];
		$db = null;
		print_debug($serverId);

	} catch (PDOExecption $e) {
		echo "Exception: ".$e->getMessage();
	}

	// TODO: Increment capture count on remote database
	$result = curlRemote("stats-manager.php", array(
		"option" => "increment_capture_count",
		"server_id" => $serverId
	));

	print_debug($result);
} else {
	echo "Invalid option";
}

?>