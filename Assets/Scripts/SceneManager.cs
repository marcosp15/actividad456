using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colores;
    private Material materialcubos;

    private GameObject cubo;
    private GameObject miCamara;

    private Vector3 newPosition;
    private Vector3 newRotation;
    private Vector3 newScale;

    private Vector3 cameraPosition;
    private Vector3 cameraTarget;
    private Vector3 cameraUp;

    private float yaw = 0f;   // Rotación izquierda/derecha
    private float pitch = 0f; // Rotación arriba/abajo
    private float mouseSensitivity = 2f;
    private float moveSpeed = 5f;

    private Vector3 puntoFocal = Vector3.zero; // punto a orbitar
    private float distancia = 5f;

    private float keyboardSensitivity = 60f;
    private float zoomSpeed = 2f;
    private float minDist = 2f;
    private float maxDist = 20f;

    void Start()
    {
        newPosition = Vector3.zero;
        newRotation = Vector3.zero;
        newScale = Vector3.one;
        cameraPosition = new Vector3(0, 2, 5);
        cameraTarget = Vector3.zero;
        cameraUp = Vector3.up;

        CreateModel();
        CreateCamera();
    }
    // Update is called once per frame
    void Update()
    {
        CamaraInput();
        RecalcularMatrices();
    }

    private void CreateModel()
    {
        cubo = new GameObject("Cubo");
        MeshFilter mf = cubo.AddComponent<MeshFilter>();
        MeshRenderer mr = cubo.AddComponent<MeshRenderer>();

        Mesh malla = new Mesh();

        //arreglo de posiciones de vertices 
        vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f), // 0
            new Vector3( 0.5f, -0.5f, -0.5f), // 1
            new Vector3( 0.5f,  0.5f, -0.5f), // 2
            new Vector3(-0.5f,  0.5f, -0.5f), // 3
            new Vector3(-0.5f, -0.5f,  0.5f), // 4
            new Vector3( 0.5f, -0.5f,  0.5f), // 5
            new Vector3( 0.5f,  0.5f,  0.5f), // 6
            new Vector3(-0.5f,  0.5f,  0.5f)  // 7
        };

        triangles = new int[]
        {
            4, 5, 6, 4, 6, 7, // Frente
            1, 0, 3, 1, 3, 2, // Atrás
            0, 4, 7, 0, 7, 3, // Izquierda
            5, 1, 2, 5, 2, 6, // Derecha
            3, 7, 6, 3, 6, 2, // Arriba
            0, 1, 5, 0, 5, 4  // Abajo
        };

        colores = new Color[]
        {
            new Color(1,0,1),
            new Color(0,0,0.7f),
            new Color(0.7f,1,1),
            new Color(1,0.7f,0.4f),
            new Color(0,0,1),
            new Color(0,1,1),
            new Color(1,0,1),
            new Color(0.5f,1,1)
        };

        malla.vertices = vertices;
        malla.triangles = triangles;
        malla.colors = colores;

        mf.mesh = malla;

        materialcubos = new Material(Shader.Find("ShaderBasico"));
        mr.material = materialcubos;
    }

    private void CreateCamera()
    {
        miCamara = new GameObject();
        miCamara.AddComponent<Camera>();
        //miCamara.transform.position = new Vector3(0, 0, -15);
        //miCamara.transform.rotation = Quaternion.Euler(0, 0, 0);

        miCamara.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        miCamara.GetComponent<Camera>().backgroundColor = Color.black;
    }

    private void RecalcularMatrices()
    {
        //Calculamos la matriz de modelado 
        Matrix4x4 modelMatrix = CreateModelMatrix(newPosition, newRotation, newScale);

        //Le decimos al shader que utilice esta matriz de modelado 
        cubo.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", modelMatrix);

        //Vector3 pos = new Vector3(4, 2, 0);
        //Vector3 target = new Vector3(0, 0, 0);
        //Vector3 up = new Vector3(0, 0, 1);

        Vector3 pos = cameraPosition;
        Vector3 target = cameraTarget;
        Vector3 up = cameraUp;

        Matrix4x4 viewMatrix = CreateViewMatrix(pos, target, up);
        cubo.GetComponent<Renderer>().material.SetMatrix("_ViewMatrix", viewMatrix);

        float fov = 75f;
        float aspect = (float)Screen.width / (float)Screen.height;
        float near = 0.1f;
        float far = 100f;

        // Calcular la matriz de proyección
        Matrix4x4 projectionMatrix = CalculatePerspectiveProjectionMatrix(fov, aspect, near, far);

        // Enviar al shader
        cubo.GetComponent<Renderer>().material.SetMatrix("_ProjectionMatrix", GL.GetGPUProjectionMatrix(projectionMatrix, true));
    }

    private Matrix4x4 CreateViewMatrix(Vector3 pos, Vector3 target, Vector3 up)
    {
        Vector3 forward = (target - pos).normalized;
        Vector3 derecha = Vector3.Cross(forward, up).normalized;
        Vector3 nuevoUp = Vector3.Cross(forward, derecha);

        Matrix4x4 matriz = new Matrix4x4
        (
            new Vector4(derecha.x, derecha.y, derecha.z, -Vector3.Dot(derecha,pos)),
            new Vector4(nuevoUp.x, nuevoUp.y, nuevoUp.z, -Vector3.Dot(nuevoUp,pos)),
            new Vector4(-forward.x, -forward.y, -forward.z, Vector3.Dot(forward,pos)),
            new Vector4(0,0,0,1)
        );

        return matriz.transpose;
    }

    private Matrix4x4 CalculatePerspectiveProjectionMatrix(float fov, float aspect, float near, float far)
    {
        float fovRad = fov * Mathf.Deg2Rad;
        float f = 1f / Mathf.Tan(fovRad / 2f);

        Matrix4x4 projection = new Matrix4x4();

        projection[0, 0] = f / aspect;
        projection[0, 1] = 0f;
        projection[0, 2] = 0f;
        projection[0, 3] = 0f;

        projection[1, 0] = 0f;
        projection[1, 1] = f;
        projection[1, 2] = 0f;
        projection[1, 3] = 0f;

        projection[2, 0] = 0f;
        projection[2, 1] = 0f;
        projection[2, 2] = (far + near) / (near - far);
        projection[2, 3] = (2f * far * near) / (near - far);

        projection[3, 0] = 0f;
        projection[3, 1] = 0f;
        projection[3, 2] = -1f;
        projection[3, 3] = 0f;

        return projection; // <-- no transpuesta
    }


    private Matrix4x4 CreateModelMatrix(Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
    {
        Matrix4x4 positionMatrix = new Matrix4x4(
            new Vector4(1f, 0f, 0f, newPosition.x), // Primera columna 
            new Vector4(0f, 1f, 0f, newPosition.y), // Segunda columna 
            new Vector4(0f, 0f, 1f, newPosition.z), // Tercera columna 
            new Vector4(0f, 0f, 0f, 1f) // Cuarta columna 
        );
        positionMatrix = positionMatrix.transpose;

        Matrix4x4 rotationMatrixX = new Matrix4x4(
            new Vector4(1f, 0f, 0f, 0f), // Primera columna 
            new Vector4(0f, Mathf.Cos(newRotation.x), -Mathf.Sin(newRotation.x), 0f), // Segunda columna 
            new Vector4(0f, Mathf.Sin(newRotation.x), Mathf.Cos(newRotation.x), 0f), // Tercera columna 
            new Vector4(0f, 0f, 0f, 1f) // Cuarta columna 
        );
        Matrix4x4 rotationMatrixY = new Matrix4x4(
            new Vector4(Mathf.Cos(newRotation.y), 0f, Mathf.Sin(newRotation.y), 0f), // Primera columna 
            new Vector4(0f, 1f, 0f, 0f), // Segunda columma 
            new Vector4(-Mathf.Sin(newRotation.y), 0f, Mathf.Cos(newRotation.y), 0f), // Tercera columna 
            new Vector4(0f, 0f, 0f, 1f) // Cuarta columna 
        );
        Matrix4x4 rotationMatrixZ = new Matrix4x4(
        new Vector4(Mathf.Cos(newRotation.z), -Mathf.Sin(newRotation.z), 0f, 0f), // Primera columna 
        new Vector4(Mathf.Sin(newRotation.z), Mathf.Cos(newRotation.z), 0f, 0f), // Segunda columna 
        new Vector4(0f, 0f, 1f, 0f), // Tercera columna 
        new Vector4(0f, 0f, 0f, 1f) // Cuarta columna 
        );

        Matrix4x4 rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
        rotationMatrix = rotationMatrix.transpose;

        Matrix4x4 scaleMatrix = new Matrix4x4(
        new Vector4(newScale.x, 0f, 0f, 0f), // Primera columna 
        new Vector4(0f, newScale.y, 0f, 0f), // Segunda columna 
        new Vector4(0f, 0f, newScale.z, 0f), // Tercera columna 
        new Vector4(0f, 0f, 0f, 1f) // Cuarta columna 
        );
        scaleMatrix = scaleMatrix.transpose;

        Matrix4x4 finalMatrix = positionMatrix;
        finalMatrix *= rotationMatrix;
        finalMatrix *= scaleMatrix;
        return (finalMatrix);
    }

    private void CamaraInput()
    {
        // Movimiento con teclado
        Vector3 forward = (cameraTarget - cameraPosition).normalized;
        Vector3 right = Vector3.Cross(forward, cameraUp).normalized;

        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movement += forward;
        if (Input.GetKey(KeyCode.S)) movement -= forward;
        if (Input.GetKey(KeyCode.D)) movement += right;
        if (Input.GetKey(KeyCode.A)) movement -= right;
        if (Input.GetKey(KeyCode.R)) movement -= cameraUp;
        if (Input.GetKey(KeyCode.F)) movement += cameraUp;

        puntoFocal += movement * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow)) pitch -= keyboardSensitivity * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) pitch += keyboardSensitivity * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) yaw += keyboardSensitivity * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) yaw -= keyboardSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        //zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distancia -= scroll * zoomSpeed;
        distancia = Mathf.Clamp(distancia, minDist, maxDist);

        //pos de la camara segun angulos
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * new Vector3(0, 0, -distancia);

        //cameraPosition += movement * moveSpeed * Time.deltaTime;
        //cameraTarget += movement * moveSpeed * Time.deltaTime;

        cameraPosition = puntoFocal + offset;
        cameraTarget = puntoFocal;

        // Rotación con el mouse
        if (Input.GetMouseButton(1)) // Botón derecho del mouse
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mouseX;
            pitch -= mouseY;

            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }
    }
}
