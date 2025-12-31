Sistema de Reservas para Restaurantes (Solo Gestión por Administradores) 
Objetivo: 
Desarrollar una aplicación de escritorio que permita a los administradores del restaurante gestionar información de clientes, las reservas de mesas y Ia disponibilidad en tiempo real, asegurando que solo el personal del restaurante pueda realizar las reservas, consultas y modificaciones. 

Requerimientos Funcionales: 
I. Registro de Clientes 
Descripción: Los administradores deben poder registrar clientes en el sistema, ingresando su nombre completo, correo electrónico y teléfono (opcional). 
Detalles: Los clientes deben tener un perfil registrado en el sistema que incluya: nombre, correo electrónico, número de teléfono y un historial de reservas. Los administradores tienen la capacidad de consultar, modificar y elim inar la información de los clientes registrados. 
Criterios de Aceptación: El administrador puede ingresar la información del cliente: nombre, correo electrónico y teléfono. El sistema debe validar que el correo electrónico no esté registrado previamente.  Los administradores pueden editar o eliminar los registros de clientes.  El administrador puede consultar el historial de reservas de cada cliente. 

2. Gestión de Reservas de Mesas 
Descripción: Los administradores deben poder realizar reservas de mesas para los clientes, seleccionando la fecha, hora y número de personas. 
Detalles: Los administradores pueden realizar reservas en nombre de los clientes, asignando mesas disponibles y gestionando la ocupación. El sistema debe verificar si hay mesas disponibles para el número de personas y la hora solicitada. 
Criterios de Aceptación: EI administrador debe poder elegir un cliente reg!strado y realizar la reserva de una mesa en su nombre. El sistema debe mostrar las mesas disponibles y permitir seleccionar la mesa y el horario de la reserva. El sistema debe generar una confirmación de la reserva y asignar la mesa seleccionada. El administrador puede modificar o cancelar la reserva en cualquier momento, liberando la mesa. 

3. Consultas de Disponibilidad de Mesas 
Descripción: Los administradores deben poder consultar la disponibilidad de mesas en tiempo real, para asegurar que no se realicen reservas dobles. 
Detalles: Los administradores pueden ver la disponibilidad de mesas por fecha y hora. El sistema debe mostrar las mesas disponibles para el número de personas requerido y en el horario seleccionado. 
Criterios de Aceptación: EI administrador puede acceder a un calendario con Ia disponibilidad de mesas para diferentes fechas y horarios. EI sistema debe indicar cuántas mesas están disponibles para cada horario y número de personas. Si todas las mesas están ocupadas para una fecha o hora seleccionada, el sistema debe mostrar un mensaje indicando que no hay disponibilidad. 

4. Gestión de Reservas por Administradores 
Descripción: Los administradores deben tener la capacidad de consultar, modificar o cancelar cualquier reserva realizada en el restaurante. 
Detalles: Los administradores pueden ver una lista de todas las reservas del día, con detalles como el nombre del cliente, la fecha y la hora de la reserva, y el número de personas. Los administradores pueden modificar la reserva (cambiar la hora, número de personas, o mesa asignada). Los administradores pueden cancelar la reserva, liberando la mesa para ese horario. 
Criterios de Aceptación: Los administradores pueden ver una lista de todas las reservas confirmadas, con detalles de cada cliente. Los administradores pueden modificar la información de la reserva: cambiar la hora, el número de personas, o la mesa asignada. Los administradores pueden cancelar cualquier reserva y liberar las mesas. Las reservas modificadas qcanceladas deben ser reflejadas en tiempo real en el sistema. 

5. Notificaciones de Confirmación (Solo para el Administrador) 
Descripción: Cuando el administrador realiza una reserva o modifica una, el cliente debe recibir un correo electrónico con los detalles de la reserva. 
Detalles: El administrador puede enviar un correo de confirmación al cliente con los detalles de la reserva realizada o modificada. El correo incluirá el nombre del cliente, fecha, hora, número de personas y mesa asignada.
Criterios de Aceptación: El sistema debe enviar automáticamente un correo electrónico al cliente después de que el administrador haya realizado una reserva. El correo debe incluir todos los detalles importantes de la reserva (fecha, hora, mesa, número de personas). Si la reserva es modificada, el cliente debe recibir un correo con los nuevos detalles. 

6. Historial de Reservas Descripción: Los administradores pueden consultar el historial de reservas previas de los clientes. 
Detalles: Los administradores pueden acceder a una lista completa de todas las reservas realizadas por cada cliente, ordenadas por fecha. El historial incluirá información sobre la fecha, hora, número de personas y mesa asignada. 
Criterios de Aceptación: El administrador puede ver todas las reservas pasadas de un cliente específico, con detalles como fecha, hora, número de personas y estado (confirmada, cancelada). El administrador puede filtrar las reservas por fecha o por cliente. 

Requerimientos No Funcionales: Interfaz de Usuario (UI): La interfaz debe ser sencilla, clara y fácil de usar para los administradores del restaurante. Los administradores deben tener un acceso rápido a las funciones de registro de clientes, reservas, y consulta de disponibilidad. 

El sistema debe permitir consultas de disponibilidad en tiempo real y asegurar que las reservas sean únicas para cada mesa y hora. 