
# Revit add-in
* User selects multiple possible shaft locations as nodes
* User draws corridors with line
* User clicks on "Submit button"

# Dynamo script
* Place VAV Boxes

# Revit add-in
* Generate a mesh of nodes and edge connections
* Convert mesh into JSON
* Convert geometry into lines for rendering in Python (either JSON or svg)
* Save JSON to Google Drive

# Python script
* Read JSON
* Calculate all possible paths
* Calculate metrics (e.g. pounds of sheet metal, static pressure drop, etc.)
* User views ductwork layout options
* User selects preferred duct layout options
* Python saves nodes for preferred duct layout option to JSON

# Revit add-in
* Import JSON
* Create ductwork layout on nodes
