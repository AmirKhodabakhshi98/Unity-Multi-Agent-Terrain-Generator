using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;



//Attach to terrain object and set parameters.
public class agent : MonoBehaviour
{
    // Start is called before the first frame update

    private float[,] heightmap;
    
    public int size;
    public int depth;


    //fill in print path
    private const string arrPath = @"";



    void Start()
    {
          AgentTerrainGenerator();

    }

    
    //Initializes height map to random values
    //Smooths terrain
    //Adds mountain ranges
    //Adds rivers
    //Creates terraindata and shapes attached terrainobject.
    //Heightmap array printed to path
    private void AgentTerrainGenerator()
    {


        Initialize();
        SmoothingAgent();
        MountainAgent();
        RiverAgent();
        Generate();
    //    PrintArray();

    }

    private void Initialize()
    {
        heightmap = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                heightmap[i, j] = Random.Range(0f,1f);
            }
        }
    }



    public int NbrOfRivers; //desired nbr of rivers. river length can vary, no control over this
    public int riverDirectionChangeStep;

    //Starts from a random edge of the map and goes inwards until it reaches a mountain.
    //Starts moving toward opposite direction of map for first "riverDirectionChangeStep" steps
    //Then turns after every random nbr of steps. Random range between 1 and "riverDirectionChangeStep"
    private void RiverAgent()
    {

        int steps = riverDirectionChangeStep;

        while (NbrOfRivers > 0)
        {

        
            //river is started from a random point on a random edge of map
            (int riverX, int riverY, EdgeEnum edge, int riverDirection) = GetRandomEdgePoint();

            
            float height = 0;

            int stepCounter = 0;


            while (height < mountainMinHeight)
            {

                heightmap[riverX, riverY] = 0;

                (int x, int y, int direction ) = Traverse(riverX, riverY, riverDirection);

                if(stepCounter % steps == 0)
                {
                    direction = Random.Range(0, 4);
                    steps = Random.Range(1, riverDirectionChangeStep + 1);
                    
                }

                riverX = x;
                riverY = y;
                riverDirection = direction;
                height = heightmap[x, y];


                stepCounter++;

                 
            }

            NbrOfRivers--;

        }

    }

    //moves the x,y coordinate 1 step toward a given direction. left/up/right/down
    private (int x, int y, int direction) Traverse(int x, int y, int direction)
    {
        int newDirection = direction;

        //left
        if (direction == 0)
        {
            if (y > 0)
            {
                y--;
            }
            else //return new direction if hit the edge
            {
                newDirection = Random.Range(1, 4);
            }
        }
        //up
        else if (direction == 1)
        {
            if (x > 0)
            {
                x--;
            }
            else
            {
                int[] dirArr = { 0, 2, 3 };
                newDirection = dirArr[Random.Range(0, 3)];

            }
        }
        //right
        else if (direction == 2)
        {
            if (y < size - 1)
            {
                y++;
            }
            else
            {
                int[] dirArr = { 0, 1, 3 };
                newDirection = dirArr[Random.Range(0, 3)];

            }
        }
        //down
        else if (direction == 3)
        {
            if (x < size - 1)
            {
                x++;
            }
            else
            {
                newDirection = Random.Range(0, 3);
            }
        }

        return (x, y, newDirection);
    }




  
    //TODO: change directions from 0-3 to enum
    //enum class for directions
    private enum Directions
    {
        Left,
        Up,
        Right,
        Down

    }



    //returns a random point on the edges of the map
    private (int x, int y, EdgeEnum edge, int direction) GetRandomEdgePoint()
    {
        int rndEdge = Random.Range(0, 4);
        int x = 0, y = 0;
        int direction = -1;

        EdgeEnum edge = EdgeEnum.Left;

        //left edge
        if (rndEdge == 0)
        {
            x = Random.Range(0, size);
            y = 0;
            edge = EdgeEnum.Left;
            direction = 2; //sets direction that river should start in to opposite side of edge, i.e here right. nbrs based on traverse method

        } //top
        else if (rndEdge == 1)
        {
            x = 0;
            y = Random.Range(0, size);
            edge = EdgeEnum.Top;
            direction = 3; //down

        }//right
        else if (rndEdge == 2)
        {
            x = Random.Range(0, size);
            y = size - 1;
            edge = EdgeEnum.Right;
            direction = 0; //left


        }//bottom
        else if (rndEdge == 3)
        {
            x = size - 1;
            y = Random.Range(0, size);
            edge = EdgeEnum.Bottom;
            direction = 1; // up

        }
        return (x, y, edge, direction);
    }


    //enum classes for edges
    private enum EdgeEnum
    {
        Left,
        Top,
        Right,
        Bottom
    }





    public int SmoothingAgentToken;
    public int SmoothingAgentJumpIntervall;

    //smooths out points based on their von neumann neighbourhoods average.
    //Each smoothed point costs 1 token.
    //Every jump intervall it skips to a random point in the map and start smoothing that area.
    private void SmoothingAgent()
    {
    //    int[] rndArray = RndArray();

        /*
        for(int i=0; i<size; i++)
        {
            for(int j=0; j<size; j++)
            {
                heightmap[rndArray[i], j] = SmoothPoint(rndArray[i], j);
            }

        }
        */
        int i = Random.Range(0, size);
        int j = Random.Range(0, size);

        //smooths random points and their neighbours whilst action tokens left
        while (SmoothingAgentToken > 0)
        {

            //smooths a given point and returns its neighbour
            SmoothPoint(i, j);
            (int x, int y) = GetPointNeighbour(i,j);

            i = x;
            j = y;
            
            //every jump intervall, jump around to a random point
            if (SmoothingAgentToken % SmoothingAgentJumpIntervall == 0)
            {
               i = Random.Range(0, size);
               j = Random.Range(0, size);
            }

            SmoothingAgentToken--;
        }


    }


    //returns a random neighbouring point of input point
    private (int x, int y) GetPointNeighbour(int i, int j)
    {
        int x = i;
        int y = j;

        //"center" point
        if (i > 0 && i < size - 1 && j > 0 && j < size - 1)
        {
            int rnd = Random.Range(0, 4);

            if (rnd == 0)
            {
                x = i - 1;              
            }else if (rnd == 1)
            {
                x = i + 1;         
            }
            else if(rnd == 2)
            {           
                y = j+1;
            }
            else if (rnd == 3)
            {
                y = j-1;
            }


        }
        //top left corner
        else if (i < 1 && j < 1)
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                x = i + 1;
            }
            else if (rnd == 1)
            {
                y = j + 1;
            }
        }
        //top right corner
        else if (i == 0 && j == size - 1)
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                x = i + 1;
            }
            else if (rnd == 1)
            {
                y = j - 1;
            }


        }
        //bottom left corner
        else if (i == size - 1 && j == 0)
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                x = i - 1;

            }
            else if (rnd == 1)
            {
                y = j + 1;

            }



        }
        //bottom right corner
        else if (i == size - 1 && j == size - 1)
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                x = i - 1;
            }
            else if (rnd == 1)
            {
                y = j - 1;
            }


        }
        //top edge
        else if (i < 1)
        {
            int rnd = Random.Range(0, 3);
            if (rnd == 0)
            {
                x = i + 1;
            }
            else if (rnd == 1)
            {
                y = j - 1;
            }
            else if (rnd == 2)
            {
                y = j + 1;

            }



        }
        //bottom edge
        else if (i == size - 1)
        {
            int rnd = Random.Range(0, 3);
            if (rnd == 0)
            {
                x = i - 1;
            }
            else if (rnd == 1)
            {
                y = j - 1;
            }
            else if (rnd == 2)
            {
                y = j + 1;
            }


        }
        //left edge
        else if (j < 1)
        {
            int rnd = Random.Range(0, 3);
            if (rnd == 0)
            {
                x = i + 1;
            }
            else if (rnd == 1)
            {
                x = i - 1;
            }
            else if (rnd == 2)
            {
                y = j + 1;
            }


        }
        //right edge
        else if (j == size - 1)
        {
            int rnd = Random.Range(0, 3);
            if (rnd == 0)
            {
                x = i + 1;
            }
            else if (rnd == 1)
            {
                x = i - 1;
            }
            else if (rnd == 2)
            {
                y = j - 1;
            }
        }


        return (x, y);
    }


    //calculates von neumann based average to smooth a point, no wraparound.
    private void SmoothPoint(int i, int j)
    {
        float value = 0;

        //"center" point
        if(i>0 && i<size-1 && j>0 && j < size - 1)
        {
            value = heightmap[i + 1, j] + heightmap[i - 1, j] + heightmap[i, j + 1] + heightmap[i, j - 1] + heightmap[i,j];
            value /= 5;

        }
        //top left corner
        else if (i < 1 && j < 1)
        {
            value = heightmap[i + 1, j] + heightmap[i, j + 1] + heightmap[i, j];
            value /= 3;

        }
        //top right corner
        else if (i == 0 && j == size - 1)
        {
            value = heightmap[i, j - 1] + heightmap[i + 1, j] + heightmap[i, j];
            value /= 3;

        }
        //bottom left corner
        else if (i == size-1 && j == 0)
        {
            value = heightmap[i - 1, j] + heightmap[i, j + 1] + heightmap[i, j];
            value /= 3;

        }
        //bottom right corner
        else if(i == size-1 && j == size-1)
        {
            value = heightmap[i - 1, j] + heightmap[i, j - 1] + heightmap[i, j];
            value /= 3;
        }
        //top edge
        else if (i < 1)
        {
            value = heightmap[i, j - 1] + heightmap[i, j + 1] + heightmap[i + 1, j] + heightmap[i, j];
            value /= 4;
        }
        //bottom edge
        else if(i == size-1 )
        {
            value = heightmap[i, j - 1] + heightmap[i, j + 1] + heightmap[i - 1, j] + heightmap[i, j];
            value /= 4;

        }
        //left edge
        else if(j < 1)
        {
            value = heightmap[i,j+1] + heightmap[i-1,j] + heightmap[i+1,j] + heightmap[i, j];
            value /= 4;


        }
        //right edge
        else if(j == size - 1)
        {
            value = heightmap[i, j - 1] + heightmap[i - 1, j] + heightmap[i + 1, j] + heightmap[i, j];
            value /= 4;

        }

        //possible to introduce random +- here

        heightmap[i, j] = value;

    }









    public float mountainMinHeight; //min rnd height limit of a peak
    public float mountainMaxHeight; // max rnd height limit of a peak
    public int mountainAgentToken; //action limit for mountain agent
    public int mountainMinRndStep; //min rnd steps after which direction changes
    public int mountainMaxRndStep; //max rnd steps after which direction changes
    public int mountainAgentJumpIntervall; //jumps after this many steps to a new point to start a mountain there


    //Starts at random point and raises points to an intervall near max
    //Smooths out nearby points to increase "mountain effect"
    //
    private void MountainAgent()
    {
        int x ;
        int y ;
        int direction;

        int steps;

        x = Random.Range(0, size);
        y = Random.Range(0, size);


        while (mountainAgentToken > 0)
        {
            steps = Random.Range(mountainMinRndStep, mountainMaxRndStep);
            
            direction = Random.Range(0, 4);

            //move in a certain direction steps number of times
            while (steps > 0 && mountainAgentToken > 0)
            {

                CreateMountainPeak(x, y, direction);  //elevate a point and smooth out surrounding points  
                (int i, int j, int dir) = Traverse(x, y, direction); //move point in given direction
                x = i;
                y = j;
                direction = dir;



                //every jump intervall roll new point to start a new mountain range from there
                if(mountainAgentToken % mountainAgentJumpIntervall == 0)
                {
                    x = Random.Range(0, size);
                    y = Random.Range(0, size);
                }

                steps--;
                mountainAgentToken--;
            }

        }

    }


    public float mountainNeighbourMultiplier;
    //sets given point to value within mountain height range.
    //Lowers 2 neighbouring points perpendicular to movement direction.
    //(might in certain cases create a "gap" in mountain ridge??)
    private void CreateMountainPeak(int x, int y, int direction)
    {

        heightmap[x, y] = Random.Range(mountainMinHeight, mountainMaxHeight);

        //left or right i.e horizontal movement. perpendicular points being above and below
        if (direction == 0 || direction == 2)
        {   
            //check so the perpendicular neighbour isnt out of bounds.
            if (x > 0)
            {
                heightmap[x - 1, y] *= mountainNeighbourMultiplier; 
            }
            if (x < size - 1)
            {
                heightmap[x + 1, y] *= mountainNeighbourMultiplier;
            }
        }
        //up or down i.e vertical movement, perpendicular points being to the left and right
        else if (direction == 1 || direction == 3)
        {
            if (y > 0)
            {
                heightmap[x, y - 1] *= mountainNeighbourMultiplier;
            }
            if (y < size - 1)
            {
                heightmap[x, y + 1] *= mountainNeighbourMultiplier;
            }

        }



    }






    private void Generate()
    {


        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        
    }

    private TerrainData GenerateTerrain(TerrainData terrainData)
    {

        terrainData.heightmapResolution = size + 1;

        terrainData.size = new Vector3(size, depth, size);

        terrainData.SetHeights(0, 0, heightmap);

        return terrainData;
    
    }



    private void PrintArray()
    {
        StringBuilder sb = new StringBuilder();

        for (int i=0; i<size; i++)
        {
            sb.Append("{");
            for (int j=0; j<size; j++)
            {
                sb.Append(heightmap[i, j] + ", ");
            }
            sb.Append("}\n");
        }


        File.WriteAllText(arrPath, sb.ToString());

    }


}




/*
private (int x, int y) GetRandomMountainPeak()
{
 //   int x = 0;
 //   int y = 0;

//   bool mountainFound = false;
    while (true)
    {
        int x = Random.Range(0, size);
        int y = Random.Range(0, size);

        for (int i = x; i < size; i++)
        {
            for (int j = y; j < size; j++)
            {
                if (heightmap[i, j] >= mountainMinHeight)
                {
                    return (i, j);
                }
            }
        }           

    }

//     return (x, y);
}
*/








/*
//moves position in given direction. if point tries to go out of bounds new direction will be rolled.
private (int x, int y, int direction)TraverseMountain(int x, int y, int direction)
{
    int newDirection = direction;

    //left
    if(direction == 0)
    {
        if (y > 0)
        {
            y--;
        }
        else //return new direction if hit the edge
        {
            newDirection = Random.Range(1, 4);
        }
    }
    //up
    else if(direction == 1)
    {
        if (x > 0)
        {
            x--;
        }
        else 
        {
            int[] dirArr = { 0, 2, 3 };
            newDirection = dirArr[Random.Range(0, 3)];

        }
    }
    //right
    else if(direction == 2)
    {
        if (y < size - 1)
        {
            y++;
        }
        else
        {
            int[] dirArr = { 0, 1, 3 };
            newDirection = dirArr[Random.Range(0, 3)];

        }
    }
    //down
    else if (direction == 3)
    {
        if (x < size - 1)
        {
            x++;
        }
        else
        {
            newDirection = Random.Range(0, 3);
        }
    }

    return (x, y, newDirection);
}




    //returns an array of rnd values from 0 to size
    //obsolete(?)
    private int[] RndArray()
    {
        int[] array = new int[size];

        for(int i = 0; i<size; i++ ){
            array[i] = i;
        }

        System.Random rnd = new System.Random();
        array = array.OrderBy(x => rnd.Next()).ToArray();

        return array;
    }





*/
