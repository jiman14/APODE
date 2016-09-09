# APODE

## Introducción

APODE es un lanzador de aplicaciones construído siguiendo un modelo de programación orientado a datos estructurados.

La aplicación consta de dos motores, uno de ejecución de programas y otro que actúa de manejador de vistas, este último permite la carga dinámica de controles gráficos.

Para construir una aplicación ejecutable es necesario definir las siguientes entidades en objetos Json: programas, procesos, vistas y controles.

El código actual está escrito bajo Visual Studio 2015 en c# .Net 4.5 y usa Json para almacenar toda la información. Transcribirlo a javascript bajo nodejs será un próximo hito.

## Manual de uso

Para iniciar un proyecto en blanco:
  1. Descomprimir el paquete "APODE - proyecto vacío.rar" que hay en la raiz del proyecto.
  2. Abrir la solución con Visual Studio 2015
  3. Compilar la solución

A continuación en este manual se describen las entidades básicas y la metodología de programación.

**La carpeta AppData contiene una aplicación de prueba** de gestión de notas (Postit), para iniciarla:

  1. Descargar o clonar el repositorio en local
  2. Abrir la solución con Visual Studio 2015
  3. Compilar la solución y ejecutar

El proyecto principal es el llamado “APODE_Core”, este proyecto contiene la funcionalidad necesaria para lanzar aplicaciones.

Las aplicaciones están definidas en dos ubicaciones:
- La carpeta ``` \AppData ```
- El fichero ``` \APODE_Core\Logic\CLogic.cs ```

El código de la aplicación se encuentra dentro de un solo fichero en el CORE para facilitar las labores de depuración y la inserción de código nuevo sin detener el depurador.


## Entidades de APODE

Permitir aportar información a determinadas entidades ayuda a enriquecer el sistema, facilitando por ejemplo tareas transversales como la trazabilidad, el control de excepciones, los análisis previos a la ejecución y las analíticas para la eficiencia del código entre otros.

Los descriptores de las entidades se deben jerarquizar siguiendo el esquema desarrollado en el análisis funcional inicial. De manera que el resultado de dicho análisis se materialice en el germen de la aplicación.


#### **Programas**

Se usa para describir la lógica de la aplicación a alto nivel, la organización jerárquica es libre. Un posible esquema sería el siguiente:

- Nombre del módulo
  - Nombre de la funcionalidad
    - Nombre del programa
      - Nombre de la sub-funcionalidad
    - Nombre del programa


Toda la información añadida a esta estructura será accesible desde cada proceso:
```
{
  "$schema": "..\\Schemas\\Schema_base_program.json",
  "Name": "program name",
  "Parallel_execution": false,
  "Description": "description",
  "Variables": [ “var1”, “var2”, ... ],
  "Configuration": { “parametro1”: “valor1”,... },
  "Logic": [
    {
      "Namespace": "",     
      "Guid": "",
      "Configuration": { },
      "Inputs": { },
      "Outputs": { "result" }
    }
  ]
}
```

El campo “Variables” es un diccionario que facilita la comunicación entre los procesos. Y “Logic” mantiene una lista de procesos que se ejecutarán en el orden establecido.


#### **Procesos**

El proceso es la unidad básica y estará asociada a su código por el Guid.

La descripción de las entradas y salidas permite realizar análisis previos para evitar inconsistencia de datos. En este caso el sufijo “-” en una variable indicará la obligatoriedad de la existencia de la misma.
```
{
  "$schema": "..\\Schemas\\Schema_base_process.json",
  "description": "",
  "processes": [
    {
      "Guid": "unique id",
      "Name": "name of process",
      "Description": "description of process",
      "Version": "1.0",
      "Configuration": [ ],
      "Default_Configuration": [ ],
      "Inputs": [ ],
      "Outputs": [ ]
    }
  ]
}
```


#### **Vistas**

Se usan para describir la estructura de la interfaz de usuario. La organización jerárquica es libre. Un posible esquema sería el siguiente:

- Nombre del módulo
    - Nombre de la ventana principal
      - Nombre de la vista
      - Ventana hija
        - Nombre de la vista

El proceso puede acceder en todo momento a la vista activa, sus variables y controles. También puede acceder al resto de vistas cargadas.
```
{
  "$schema": "..\\Schemas\\Schema_base_control.json",
  "description": "view description",
  "Variables": [ “var1”, ...  ],
  "Controls": [ “guid_control_1”, ... ]
}
```
Las vistas proporcionan un diccionario de variables accesible a todos los programas que facilita la interacción entre los mismos.


#### **Controles**

El motor de la interfaz de usuario carga dinámicamente los controles, por lo que una vez cargados para ganar en velocidad es aconsejable no eliminarlos sino ocultarlos y recargar su contenido si es necesario.
El tipo del control puede hacer referencia a librerías externas que se cargarán dinámicamente.
Las propiedades de los controles están mapeadas dinámicamente, por lo que cualquier propiedad existente en el control se puede indicar y se aplicará en la creación del control.
```
{
  "Guid": "Control_guid",
  "Type": "Form",
  "Description": "description text",
  "Version": "1.0",
  "Configuration": {        
    "MinimumSize": {
      "Width": 45,
      "Height": 280
    },
    "FormBorderStyle": 0,
    "TopMost": false // Show over all
  },
  "Controls": [ child_controls_guids ],
  "Events": {      
    "Load": "MainApp.LoadApp",
  }
}
```
