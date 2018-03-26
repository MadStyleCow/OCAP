<?php
    header('Content-Type: application/json');
    $dbPath = "data/data.db";
    $db = new PDO('sqlite:' . $dbPath);
    $error = 0;
    $ops = [];
    if (!file_exists($dbPath)) {
        $error = 1;
    } else {
        // Get list of operations from DB
        // Exclude type, due to it being unecessary
        $req = "SELECT * FROM operations WHERE
            mission_name LIKE '%".addslashes($_POST["name"])."%' AND 
            date <= '".addslashes($_POST['older'])."' AND 
            date >= '".addslashes($_POST['newer'])."'";

        // Excute query
        $result = $db->query($req);

        // Parse results
        $ops = array();
        foreach($result as $row) {
            $ops[] = $row;
        }

        $db = NULL;
    };
?>
{
    "error" : <?php echo json_encode($error) ?>,
    "list"  : <?php echo json_encode($ops) ?>
}

