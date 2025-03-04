# Documento de Pruebas Unitarias para el Proyecto Sprint02Tasks

## 1. Introducción

Este documento describe las pruebas unitarias implementadas para el proyecto Sprint02Tasks.
Las pruebas unitarias son esenciales para garantizar que cada componente del software funcione correctamente de manera aislada.

A continuación, se detallan las pruebas unitarias para las funcionalidades de:

* Creación de Tareas
* Listado de Tareas
* Actualización de Tareas
* Eliminación de Tareas
* Seguridad y Manejo de Errores

## 2. Índice

1. Introducción
2. Creación de Tareas
3. Listado de Tareas
4. Actualización de Tareas
5. Eliminación de Tareas
6. Seguridad y Manejo de Errores
7. Conclusión

## 3. Creación de Tareas

### Función: crearTarea(titulo, descripcion, usuario)

| ID | Descripción | Entrada | Salida Esperada |
|----|-------------|---------|-----------------|
| CU_001 | Verificar que una tarea se crea correctamente con datos válidos | titulo = "Nueva Tarea", descripcion = "Nueva Tarea", status = Pending | Tarea creada con éxito |
| CU_002 | Verificar que se lanza una excepción al intentar crear una tarea con descripción inválida | titulo = "Corta", descripcion = "Corta", status = Pending | Excepción: La descripción debe tener al menos 10 caracteres |
| CU_003 | Verificar que se lanza una excepción al intentar crear una tarea con estado inválido | titulo = "Descripción válida de la tarea", descripcion = "Descripción válida de la tarea", status = 999 (inválido) | Excepción: El estado de la tarea no es válido |
| CU_019 | Verificar que se lanza una excepción al intentar crear una tarea con título demasiado largo | titulo = [string de longitud > MaxDescriptionLength], descripcion = [string de longitud > MaxDescriptionLength], status = Pending | Excepción: La descripción debe tener entre 10 y [MaxDescriptionLength] caracteres |

## 4. Listado de Tareas

| ID | Descripción | Entrada | Salida Esperada |
|----|-------------|---------|-----------------|
| CU_004 | Verificar que se obtiene una tarea existente por su ID | taskId = 1 | Tarea con ID=1 |
| CU_005 | Verificar que se devuelve null al buscar una tarea con ID inexistente | taskId = 999 | null |
| CU_006 | Verificar que se obtienen todas las tareas correctamente | N/A | Lista con todas las tareas |
| CU_007 | Verificar que se filtran las tareas correctamente por estado | statusFilter = Pending/InProgress/Completed | Lista filtrada por el estado correspondiente |
| CU_008 | Verificar que la búsqueda de tareas por término funciona correctamente | searchTerm = "importante" | Lista de tareas que contienen "importante" en su descripción |

## 5. Actualización de Tareas

| ID | Descripción | Entrada | Salida Esperada |
|----|-------------|---------|-----------------|
| CU_009 | Verificar que se actualiza correctamente una tarea existente | taskDto con Id=1, Description="Tarea actualizada", Status=InProgress, Priority=Alta | Tarea actualizada correctamente |
| CU_010 | Verificar que se actualiza correctamente el estado de una tarea | taskId = 1, newStatus = Completed | Estado actualizado correctamente |
| CU_011 | Verificar que se actualiza correctamente la prioridad de una tarea | taskId = 1, newPriority = Alta | Prioridad actualizada correctamente |
| CU_012 | Verificar que se añade correctamente una categoría a una tarea | taskId = 1, category = "Importante" | Categoría añadida correctamente |

## 6. Eliminación de Tareas

| ID | Descripción | Entrada | Salida Esperada |
|----|-------------|---------|-----------------|
| CU_013 | Verificar que se elimina correctamente una tarea existente | taskId = 1 | Tarea eliminada correctamente |
| CU_014 | Verificar que no se produce error al intentar eliminar una tarea inexistente | taskId = 999 | false (sin errores) |

## 7. Seguridad y Manejo de Errores

| ID | Descripción | Entrada | Salida Esperada |
|----|-------------|---------|-----------------|
| CU_015 | Verificar que se sanitiza correctamente la entrada de texto | input = "Tarea con <script>alert('XSS')</script> código malicioso" | "Tarea con  código malicioso" |
| CU_016 | Verificar que se registran correctamente los cambios en los datos | entityType = "Task", entityId = "1", changeDetails = "Updated task status to Completed", user = "testuser" | Registro correcto del cambio |
| CU_017 | Verificar que se registran correctamente las violaciones de seguridad | resource = "TaskAPI", ipAddress = "192.168.1.1", additionalInfo = "Unauthorized access attempt" | Registro correcto de la violación |
| CU_018 | Verificar que se manejan correctamente las excepciones durante las operaciones | taskDto con error que provoca excepción | Rollback de la transacción |

## 8. Conclusión

Las pruebas unitarias implementadas cubren los aspectos fundamentales del sistema de gestión de tareas, garantizando que cada componente funcione correctamente de manera aislada. Estas pruebas verifican:

1. La correcta creación de tareas con validación de datos
2. El listado y búsqueda eficiente de tareas
3. La actualización precisa de diferentes propiedades de las tareas
4. La eliminación segura de tareas
5. El manejo adecuado de la seguridad y los errores

Estas pruebas proporcionan una base sólida para asegurar la calidad del software y facilitar futuras mejoras y mantenimiento.
