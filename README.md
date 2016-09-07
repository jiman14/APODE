# APODE

## Introducción

APODE es un lanzador de aplicaciones construído siguiendo un modelo de programación orientado a datos estructurados.

La aplicación consta de dos motores, uno de ejecución de programas y otro que actúa de manejador de vistas, este último permite la carga dinámica de controles gráficos. 

Para construir una aplicación ejecutable es necesario definir las siguientes entidades en objetos Json: programas, procesos, vistas y controles.

El código actual está escrito bajo Visual Studio 2015 en c# .Net 4.5 y usa Json para almacenar toda la información. Transcribirlo a javascript bajo nodejs será un próximo hito.

## Contenido del proyecto

El proyecto principal es el llamado “APODE_Core”, este proyecto contiene la funcionalidad necesaria para lanzar aplicaciones.

Las aplicaciones están definidas en dos ubicaciones:
- La carpeta \AppData
- El fichero \APODE_Core\Logic\CLogic.cs *

*El código de la aplicación se encuentra dentro de un solo fichero en el CORE para facilitar las labores de depuración y la inserción de código nuevo sin detener el depurador.

## Uso

La aplicación de ejemplo es una aplicación de gestión de notas de escritorio. Para verla descargar el código, abrir con Visual Studio 2015 y ejecutar.

Para ejecutar una nueva aplicación utilizar el paquete "APODE - proyecto vacío.rar" que hay en la raiz del proyecto.



