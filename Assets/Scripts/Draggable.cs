using UnityEngine;

// Base class for user interactions with team's Agents
public class Draggable : MonoBehaviour {
    public Vector3 DragOffset = new Vector3(0, 0.25f, 0);

    private Camera _camera;
    private Vector3 _originPosition;

    private Tile _originTile;
    private Tile _previousTile;

    private Graph.Node _originNode;

    public bool IsDragging;

    private const int _default = 0;
    private const int _valid   = 1;
    private const int _invalid = 2;
    private const int _origin  = 3;

    private const int _leftClick = 0;
    private const int _rightClick = 1;

    // Start is called before the first frame update
    private void Start() {
        _camera = Camera.main;
    }

    // On Agent click, get parent tile and update color for user feedback
    public void OnClick() {
        if (Input.GetMouseButtonDown(_leftClick)) {
            _originTile = GetTileUnder();

            if (_originTile != null) {
                _originTile.SetHighlightColor(_origin);
                _originPosition = this.transform.position;
                _originNode = GridManager.Instance.GetNodeForTile(_originTile);
            }
        }
    }

    // On Agent release, reset tile color
    public void OnRelease() {
        if (Input.GetMouseButtonUp(_leftClick)) {
            if (_originTile != null) {
                _originTile.SetHighlightColor(_default);
                _originTile.SetAlpha(0f);
            }
        }
    }
    
    // On Agent StartDrag event, show board Tiles
    public void OnStartDrag() {
        foreach (Tile tile in GridManager.Instance.MyTiles)
            tile.SetAlpha(1f);

        foreach (Tile tile in GridManager.Instance.MyBenchTiles)
            tile.SetAlpha(0.588f);

        IsDragging = true;
        PlayerManager.Instance.SelectedAgent = this.gameObject;
    }

    // On Agent dragging event, update tile colors as Agent moves over each available tile
    public void OnDragging() {
        if (!IsDragging)
            return;

        // Need interaction with shop UI for selling Agents

        Vector3 newPosition = PlayerManager.Instance.GetMousePosition() + DragOffset;
        this.transform.position = newPosition;

        Tile tileUnder = GetTileUnder();

        if (tileUnder != null) {
            string parentName = tileUnder.transform.parent.name;

            switch (parentName) {
                case "Battle Grid":
                    // Logic for "full" team on battlefield
                    if (GameManager.Instance.CurrentTeamSize == GameManager.Instance.TeamSize) {
                        // Check if Agent's origin was on Battle Grid
                        if (this.transform.parent.parent.name == parentName) {
                            tileUnder.SetHighlightColor(_valid);
                        }
                        else {
                            tileUnder.SetHighlightColor(
                                !GridManager.Instance.GetNodeForTile(tileUnder).IsOccupied ? _invalid : _valid);
                        }
                    }
                    else {
                        tileUnder.SetHighlightColor(_valid);
                    }
                    
                    break;
                case "Player Agents":
                    tileUnder.SetHighlightColor(_valid);
                    break;
                case "Enemy Agents":    // Do not want to allow player to place agents on enemy bench
                    tileUnder.SetHighlightColor(_invalid);
                    break;
            }
            
            if (_previousTile != null && tileUnder != _previousTile) {
                _previousTile.SetHighlightColor(_previousTile == _originTile ? _origin : _default);
            }

            _previousTile = tileUnder;
        }
        else {
            if(_previousTile != null)
                _previousTile.SetHighlightColor(_previousTile == _originTile ? _origin : _default);
        }
    }

    // On Agent EndDrag event, update Agent position and reset tile colors
    public void OnEndDrag() {
        if (!IsDragging)
            return;

        if (!TryRelease())
            this.transform.position = _originPosition;

        if (_previousTile != null) {
            _previousTile.SetHighlightColor(_default);
            _previousTile = null;
        }

        foreach (Tile tile in GridManager.Instance.MyTiles)
            tile.SetAlpha(0f);

        foreach (Tile tile in GridManager.Instance.MyBenchTiles)
            tile.SetAlpha(0f);

        IsDragging = false;
        PlayerManager.Instance.SelectedAgent = null;
    }

    // ABSTRACTION
    // If Agent can be released, update parent and set Agent's new position
    private bool TryRelease() {
        var tile = GetTileUnder();
        
        // Disallow release on new tile if tile is null or "invalid"
        if (tile != null && tile.ColorIndex != _invalid) {
            Agent thisAgent = GetComponent<Agent>();
            Graph.Node targetNode = GridManager.Instance.GetNodeForTile(tile);

            if (thisAgent != null && targetNode != null) {
                // Check if team size is full
                thisAgent.CurrentNode.SetOccupied(false);

                // Swap Agent positions, nodes, parents
                if (targetNode.IsOccupied) {
                    Transform targetChild = targetNode.Parent.GetChild(0);
                    Agent targetAgent = targetChild.GetComponent<Agent>();

                    targetAgent.CurrentNode.SetOccupied(false);
                    targetAgent.SetCurrentNode(_originNode);
                    targetAgent.transform.SetPositionAndRotation(_originNode.WorldPosition,
                        Quaternion.Euler(0, _originNode.YRotation, 0));
                    _originNode.SetOccupied(true);
                }

                thisAgent.SetCurrentNode(targetNode);
                thisAgent.transform.SetPositionAndRotation(targetNode.WorldPosition,
                    Quaternion.Euler(0, targetNode.YRotation, 0));
                targetNode.SetOccupied(true);
            
                GameManager.Instance.CurrentTeamSize = GridManager.Instance.GetOccupiedTiles(GridManager.Instance.Graph,
                    GridManager.Instance.MyTiles);
                GameManager.Instance.AllowedAgentsText.text = $"{GameManager.Instance.CurrentTeamSize}" + 
                                                              $"/{PlayerManager.Instance.PlayerLevel}";

                return true;
            }
        }

        return false;
    }
    
    // ABSTRACTION
    // Obtain the tile underneath the Agent
    public Tile GetTileUnder() {
        if (!IsDragging) {
            Tile tile = this.gameObject.transform.parent.gameObject.GetComponent<Tile>();
            return tile;
        }
        else {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit)) {
                Tile tile = hit.collider.GetComponent<Tile>();
                return tile;
            }

            return null;
        }
    }
}
