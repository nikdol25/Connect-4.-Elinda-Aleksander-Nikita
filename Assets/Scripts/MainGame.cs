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
    Color currentColor = Color.red; // Initial color to spawn
    public Text gameStateText; // Reference to the UI text object for displaying game state

    public class GameStateSerializer
    {
        public static string SerializeGameState(MainGame mainGame)
        {
            StringBuilder sb = new StringBuilder();

            // Append cube positions
            foreach (GameObject cube in mainGame.cubes)
            {
                sb.Append(cube.transform.position.x).Append(",").Append(cube.transform.position.y).Append(",");
            }

            // Append sphere positions and colors
            foreach (GameObject sphere in mainGame.spheres)
            {
                sb.Append(sphere.transform.position.x).Append(",").Append(sphere.transform.position.y).Append(",")
                  .Append(sphere.GetComponent<Renderer>().material.color.r).Append(",")
                  .Append(sphere.GetComponent<Renderer>().material.color.g).Append(",")
                  .Append(sphere.GetComponent<Renderer>().material.color.b).Append(",");
            }

            // Append current sphere color and spherePlaced flag
            sb.Append(mainGame.currentColor.r).Append(",")
              .Append(mainGame.currentColor.g).Append(",")
              .Append(mainGame.currentColor.b).Append(",")
              .Append(mainGame.spherePlaced);

            return sb.ToString();
        }
    }

    public class GameStateDeserializer
    {
        public static void DeserializeGameState(string serializedGameState, MainGame mainGame)
        {
            // Clear previous game state
            mainGame.spheres.Clear();

            string[] data = serializedGameState.Split(',');

            // Retrieve cube positions
            int cubeIndex = 0;
            for (int x = 0; x < mainGame.Nx; x++)
            {
                for (int y = 0; y < mainGame.Ny; y++)
                {
                    float posX = float.Parse(data[cubeIndex]);
                    float posY = float.Parse(data[cubeIndex + 1]);
                    mainGame.cubes[x, y].transform.position = new Vector3(posX, posY, 0);
                    cubeIndex += 2;
                }
            }

            // Retrieve sphere positions and colors
            for (int i = cubeIndex; i < data.Length - 4; i += 5)
            {
                float posX = float.Parse(data[i]);
                float posY = float.Parse(data[i + 1]);
                float colorR = float.Parse(data[i + 2]);
                float colorG = float.Parse(data[i + 3]);
                float colorB = float.Parse(data[i + 4]);

                Vector3 spherePosition = new Vector3(posX, posY, 0);
                Color sphereColor = new Color(colorR, colorG, colorB);

                GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                newSphere.transform.position = spherePosition;
                newSphere.GetComponent<Renderer>().material.color = sphereColor;

                mainGame.spheres.Add(newSphere);
            }

            // Retrieve current sphere color and spherePlaced flag
            float currentColorR = float.Parse(data[data.Length - 4]);
            float currentColorG = float.Parse(data[data.Length - 3]);
            float currentColorB = float.Parse(data[data.Length - 2]);
            mainGame.currentColor = new Color(currentColorR, currentColorG, currentColorB);
            mainGame.spherePlaced = bool.Parse(data[data.Length - 1]);
        }
    }


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
                cube.transform.localScale = Vector3.one * 0.7f;
                cubes[x, y] = cube; // Store reference to the cube in the array
            }
        }

        // Creating the initial sphere and setting its initial position and color
        CreateSphere(new Vector3(0, 0, 0), currentColor);
    }

    void Update()
    {
        if (!spherePlaced)
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
                // Toggle current color between red and blue
                currentColor = (currentColor == Color.red) ? Color.blue : Color.red;

                // Create a new sphere with the current color
                CreateSphere(spheres[spheres.Count - 1].transform.position + Vector3.up * spacing, currentColor);

                // Check for winning condition after placing the sphere
                CheckWinCondition();
            }



        }

        else
        {
            // Game has ended, check for restart input
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Reset game state
                ResetGameState();
            }
        }

        UpdateGameStateText();

    }

    void UpdateGameStateText()
    {
        // Serialize the game state
        string serializedGameState = GameStateSerializer.SerializeGameState(this);

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
            sphere.transform.position = newPosition;
        }
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
        string winningPlayer = (color == Color.red) ? "Red" : "Blue";

        winTextObject.SetActive(true);
        winTextObject.GetComponent<Text>().text = "Player " + winningPlayer + " wins!";
        spherePlaced = true; // Stop further sphere placements
    }

    // Function to check if a position is within the bounds of the grid
    bool IsPositionWithinGrid(Vector3 position)
    {
        return position.x >= 0 && position.x < (Nx - 1) * spacing &&
               position.y >= 0 && position.y < (Ny - 1) * spacing;
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
        currentColor = Color.red;
        spherePlaced = false;

        // Reset initial sphere and its position
        CreateSphere(new Vector3(0, 0, 0), currentColor);
    }


}





















