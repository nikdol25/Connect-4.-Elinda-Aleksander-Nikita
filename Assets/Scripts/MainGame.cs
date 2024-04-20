using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class MainGame : MonoBehaviour
{
    public int Nx = 7; // Number of cubes along the x-axis
    public int Ny = 6; // Number of cubes along the y-axis
    public float spacing = 1.1f; // Spacing between cubes
    public GameObject winTextObject; // Reference to the UI text object for displaying win message
    GameObject[,] cubes; // 2D array to hold references to cubes
    List<GameObject> spheres = new List<GameObject>(); // List to hold references to spheres
    bool spherePlaced = false; // Flag to indicate if the sphere has been placed
    Color playerColor = Color.red; // Player's color
    Color aiColor = Color.blue; // AI's color
    public Text gameStateText; // Reference to the UI text object for displaying game state

    void Start()
    {
        cubes = new GameObject[Nx, Ny]; // Initialize the 2D array

        // Creating cubes in a grid pattern
        for (int x = 0; x < Nx; x++)
        {
            for (int y = 0; y < Ny; y++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(x * spacing, y * spacing, 0);
                cube.transform.localScale = Vector3.one * 0.5f;
                cubes[x, y] = cube; // Store reference to the cube in the array
            }
        }

        // Creating the initial sphere and setting its initial position and color
        CreateSphere(new Vector3(0, 0, 0), aiColor); // Initial sphere is blue
    }

    void Update()
    {
        if (!spherePlaced)
        {
            // Player's turn
            HandlePlayerTurn();
        }
        else
        {
            // AI's turn
            HandleAITurn();
        }

        UpdateGameStateText();
    }

    void HandlePlayerTurn()
    {
        // Moving the sphere with W, A, S, D keys
        Vector3 direction = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Vector3.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Vector3.down;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Vector3.right;
        }

        // Move the sphere if a valid direction is pressed
        if (direction != Vector3.zero)
        {
            MoveSphere(spheres[spheres.Count - 1], direction);
        }

        // Placing the sphere with 'E' key
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Create a new sphere with the player's color
            CreateSphere(spheres[spheres.Count - 1].transform.position + Vector3.up * spacing, playerColor);

            // Check for winning condition after placing the sphere
            CheckWinCondition();

            // Set spherePlaced flag to true to indicate the end of the player's turn
            spherePlaced = true;
        }
    }

    void HandleAITurn()
    {
        // Implement AI turn using the minimax algorithm
        // Placeholder code
        // AI makes a random move for now
        int bestMove = MiniMax();
        int x = bestMove % Nx;
        int y = bestMove / Nx;
        Vector3 aiMovePosition = new Vector3(x * spacing, y * spacing, 0);

        // Make sure the selected cube is not already occupied
        if (!IsCubeOccupied(aiMovePosition))
        {
            // Create a new sphere for the AI player with the AI's color
            CreateSphere(aiMovePosition, aiColor);

            // Check for winning condition after AI places the sphere
            CheckWinCondition();

            // Set spherePlaced flag to false to indicate the end of the AI's turn
            spherePlaced = false;
        }
        else
        {
            // Retry with a different random move if the selected cube is already occupied
            HandleAITurn();
        }
    }

    int MiniMax()
    {
        // Placeholder implementation of MiniMax algorithm
        // Here you would implement the MiniMax algorithm to determine the best move for the AI player
        // For now, it just returns a random valid move
        List<int> validMoves = new List<int>();
        for (int x = 0; x < Nx; x++)
        {
            for (int y = 0; y < Ny; y++)
            {
                if (!IsCubeOccupied(new Vector3(x * spacing, y * spacing, 0)))
                {
                    validMoves.Add(x + y * Nx);
                }
            }
        }
        return validMoves[Random.Range(0, validMoves.Count)];
    }

    void UpdateGameStateText()
    {
        // Serialize the game state
        string serializedGameState = SerializeGameState();

        // Update the UI text object with the serialized game state
        if (gameStateText != null)
        {
            gameStateText.text = "Game State:\n" + serializedGameState;
        }
    }

    void MoveSphere(GameObject sphere, Vector3 direction)
    {
        Vector3 newPosition = sphere.transform.position + direction * spacing;

        // Check if the new position is within the bounds of the grid
        if (IsPositionWithinGrid(newPosition))
        {
            // Check if the target cube is already occupied by a sphere
            if (!IsCubeOccupied(newPosition))
            {
                sphere.transform.position = newPosition;
            }
            else
            {
                Debug.LogWarning("Cannot move sphere. Target cube is already occupied.");
            }
        }
        else
        {
            Debug.LogWarning("Cannot move sphere. Target position is outside the grid.");
        }
    }

    // Function to check if a cube at a given position is already occupied by a sphere
    bool IsCubeOccupied(Vector3 position)
    {
        // Iterate through all spheres to check if any sphere occupies the same cube
        foreach (GameObject sphere in spheres)
        {
            // Calculate the difference in positions between the sphere and the target cube
            Vector3 positionDifference = sphere.transform.position - position;

            // Check if the position difference is very small, indicating the sphere is in the target cube
            if (positionDifference.sqrMagnitude < 0.01f) // Adjust the threshold as needed
            {
                return true;
            }
        }

        // If no sphere occupies the target cube, return false (cube is not occupied)
        return false;
    }

    // Function to create a new sphere
    void CreateSphere(Vector3 position, Color color)
    {
        GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newSphere.transform.position = position;
        newSphere.GetComponent<Renderer>().material.color = color;
        spheres.Add(newSphere);
    }

    // Function to check for winning condition
    void CheckWinCondition()
    {
        foreach (GameObject sphere in spheres)
        {
            if (CheckLineFromSphere(sphere))
            {
                NotifyWinningPlayer(sphere.GetComponent<Renderer>().material.color);
                return;
            }
        }
    }

    bool CheckLineFromSphere(GameObject sphere)
    {
        Color color = sphere.GetComponent<Renderer>().material.color;
        Vector3 position = sphere.transform.position;

        // Check for horizontal, vertical, and diagonal lines
        if (CheckLine(position, color, Vector3.right) || // Horizontal
            CheckLine(position, color, Vector3.up) ||    // Vertical
            CheckLine(position, color, new Vector3(1, 1, 0)) ||  // Diagonal \
            CheckLine(position, color, new Vector3(1, -1, 0)))    // Diagonal /
        {
            return true;
        }

        return false;
    }

    // Function to check if a line of same-colored spheres exists starting from a given position with a specified direction
    bool CheckLine(Vector3 startPosition, Color color, Vector3 direction)
    {
        int count = 0;

        // Check in the positive and negative directions
        for (int i = -3; i <= 3; i++)
        {
            Vector3 positionToCheck = startPosition + direction * i * spacing;
            if (IsPositionWithinGrid(positionToCheck) && SphereAtPosition(positionToCheck, color))
            {
                count++;
            }
            else
            {
                count = 0; // Reset the count if we encounter a gap
            }

            // If we find 4 consecutive spheres of the same color, return true
            if (count >= 4)
            {
                return true;
            }
        }

        return false;
    }

    bool SphereAtPosition(Vector3 position, Color color)
    {
        foreach (GameObject sphere in spheres)
        {
            if (sphere.transform.position == position && sphere.GetComponent<Renderer>().material.color == color)
            {
                return true;
            }
        }
        return false;
    }

    // Function to notify winning player through UI
    void NotifyWinningPlayer(Color color)
    {
        string winningPlayer = (color == playerColor) ? "AI" : "Player";

        winTextObject.SetActive(true);
        winTextObject.GetComponent<Text>().text = winningPlayer + " wins!";
        spherePlaced = true; // Stop further sphere placements
    }

    // Function to check if a position is within the bounds of the grid
    bool IsPositionWithinGrid(Vector3 position)
    {
        return position.x >= 0 && position.x < (Nx - 1) * spacing &&
               position.y >= 0 && position.y < (Ny - 1) * spacing;
    }

    string SerializeGameState()
    {
        StringBuilder sb = new StringBuilder();

        // Append cube positions
        foreach (GameObject cube in cubes)
        {
            sb.Append(cube.transform.position.x).Append(",").Append(cube.transform.position.y).Append(",");
        }

        // Append sphere positions and colors
        foreach (GameObject sphere in spheres)
        {
            sb.Append(sphere.transform.position.x).Append(",").Append(sphere.transform.position.y).Append(",")
              .Append(sphere.GetComponent<Renderer>().material.color.r).Append(",")
              .Append(sphere.GetComponent<Renderer>().material.color.g).Append(",")
              .Append(sphere.GetComponent<Renderer>().material.color.b).Append(",");
        }

        // Append current sphere color and spherePlaced flag
        sb.Append(playerColor.r).Append(",")
          .Append(playerColor.g).Append(",")
          .Append(playerColor.b).Append(",")
          .Append(spherePlaced);

        return sb.ToString();
    }

    void ResetGameState()
    {
        // Deactivate win text object
        winTextObject.SetActive(false);
        // Clear spheres list
        foreach (GameObject sphere in spheres)
        {
            Destroy(sphere);
        }
        spheres.Clear();
        // Reset current color and spherePlaced flag
        playerColor = Color.red;
        spherePlaced = false;
        // Reset initial sphere and its position
        CreateSphere(new Vector3(0, 0, 0), aiColor); // Initial sphere is blue
    }
}