-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: localhost    Database: kamerequest
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `desglosessolicitud`
--

DROP TABLE IF EXISTS `desglosessolicitud`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `desglosessolicitud` (
  `IdDesglose` int NOT NULL AUTO_INCREMENT,
  `IdSolicitud` int NOT NULL,
  `Mes` int NOT NULL,
  `Anio` int NOT NULL,
  `Horas` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Monto` decimal(12,2) NOT NULL,
  `Iva` decimal(12,2) NOT NULL,
  `Total` decimal(12,2) NOT NULL,
  `PorcentajeCobro` decimal(5,2) NOT NULL DEFAULT '100.00',
  `Observaciones` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`IdDesglose`),
  KEY `IdSolicitud` (`IdSolicitud`),
  CONSTRAINT `desglosessolicitud_ibfk_1` FOREIGN KEY (`IdSolicitud`) REFERENCES `solicitudes` (`IdSolicitud`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `desglosessolicitud`
--

LOCK TABLES `desglosessolicitud` WRITE;
/*!40000 ALTER TABLE `desglosessolicitud` DISABLE KEYS */;
INSERT INTO `desglosessolicitud` VALUES (2,2,10,2026,90.00,300000.00,39000.00,339000.00,0.00,'pasemela'),(3,3,10,2026,20.00,40000.00,5200.00,45200.00,40.00,'metela');
/*!40000 ALTER TABLE `desglosessolicitud` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `estadossolicitud`
--

DROP TABLE IF EXISTS `estadossolicitud`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `estadossolicitud` (
  `IdEstado` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`IdEstado`),
  UNIQUE KEY `uq_estados_nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `estadossolicitud`
--

LOCK TABLES `estadossolicitud` WRITE;
/*!40000 ALTER TABLE `estadossolicitud` DISABLE KEYS */;
INSERT INTO `estadossolicitud` VALUES (4,'Finalizada'),(3,'Iniciada'),(1,'Nueva'),(2,'Respondida'),(5,'Vencida');
/*!40000 ALTER TABLE `estadossolicitud` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `representantes`
--

DROP TABLE IF EXISTS `representantes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `representantes` (
  `IdRepresentante` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`IdRepresentante`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `representantes`
--

LOCK TABLES `representantes` WRITE;
/*!40000 ALTER TABLE `representantes` DISABLE KEYS */;
INSERT INTO `representantes` VALUES (1,'Carlos Alvarado','carlos.alvarado@empresa.com'),(2,'María Fernanda Rojas','mrojas@empresa.com');
/*!40000 ALTER TABLE `representantes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `solicitudes`
--

DROP TABLE IF EXISTS `solicitudes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `solicitudes` (
  `IdSolicitud` int NOT NULL AUTO_INCREMENT,
  `ConsecutivoOficio` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `DocumentoRespuesta` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DocumentoInicio` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Titulo` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Descripcion` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `IdRepresentante` int DEFAULT NULL,
  `Observaciones` text COLLATE utf8mb4_unicode_ci,
  `IdEstado` int NOT NULL DEFAULT '1',
  `FechaIngreso` date NOT NULL,
  `FechaRespuesta` date DEFAULT NULL,
  `FechaInicio` date DEFAULT NULL,
  PRIMARY KEY (`IdSolicitud`),
  UNIQUE KEY `uq_solicitudes_consecutivo` (`ConsecutivoOficio`),
  KEY `fk_solicitudes_representante` (`IdRepresentante`),
  KEY `fk_solicitudes_estado` (`IdEstado`),
  CONSTRAINT `fk_solicitudes_estado` FOREIGN KEY (`IdEstado`) REFERENCES `estadossolicitud` (`IdEstado`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_solicitudes_representante` FOREIGN KEY (`IdRepresentante`) REFERENCES `representantes` (`IdRepresentante`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `solicitudes`
--

LOCK TABLES `solicitudes` WRITE;
/*!40000 ALTER TABLE `solicitudes` DISABLE KEYS */;
INSERT INTO `solicitudes` VALUES (1,'OFI-2026-001',NULL,NULL,'Mantenimiento Servidores Core','Revisión preventiva de los servidores de base de datos principal.',1,'Urgente por cierre fiscal',5,'2026-09-15',NULL,NULL),(2,'OFI-2026-002','RESP-002.pdf','INI-002.pdf','Desarrollo Módulo Auditoría','Creación de componentes de bitácora y roles.',2,'Proyecto asignado a desarrollo',1,'2026-10-02',NULL,NULL),(3,'OFI-2026-003',NULL,NULL,'Soporte Redes Edificio B','Instalación de puntos de red categoría 6A.',1,'Pendiente de aprobación presupuestaria',1,'2026-09-25',NULL,NULL);
/*!40000 ALTER TABLE `solicitudes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tareassolicitud`
--

DROP TABLE IF EXISTS `tareassolicitud`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tareassolicitud` (
  `IdTarea` int NOT NULL AUTO_INCREMENT,
  `IdSolicitud` int NOT NULL,
  `Descripcion` varchar(500) NOT NULL,
  `Fecha` date NOT NULL,
  `Usuario` varchar(100) NOT NULL,
  `HorasInvertidas` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`IdTarea`),
  KEY `FK_Tareas_Solicitudes` (`IdSolicitud`),
  CONSTRAINT `FK_Tareas_Solicitudes` FOREIGN KEY (`IdSolicitud`) REFERENCES `solicitudes` (`IdSolicitud`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tareassolicitud`
--

LOCK TABLES `tareassolicitud` WRITE;
/*!40000 ALTER TABLE `tareassolicitud` DISABLE KEYS */;
INSERT INTO `tareassolicitud` VALUES (1,2,'Tarea que hay que hacer ','2026-10-02','Administrador del sistema',11.00);
/*!40000 ALTER TABLE `tareassolicitud` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `IdUsuario` int NOT NULL AUTO_INCREMENT,
  `NombreCompleto` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `NombreUsuario` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Correo` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `TipoUsuario` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Usuario',
  `Estado` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Activo',
  `ContrasenaCifrada` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nonce` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EtiquetaAutenticacion` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `IntentosFallidos` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`IdUsuario`),
  UNIQUE KEY `uq_usuarios_nombre` (`NombreUsuario`),
  UNIQUE KEY `uq_usuarios_correo` (`Correo`),
  CONSTRAINT `ck_usuarios_estado` CHECK ((`Estado` in (_utf8mb4'Activo',_utf8mb4'Inactivo',_utf8mb4'Bloqueado'))),
  CONSTRAINT `ck_usuarios_intentos` CHECK ((`IntentosFallidos` >= 0)),
  CONSTRAINT `ck_usuarios_tipo` CHECK ((`TipoUsuario` in (_utf8mb4'Administrador',_utf8mb4'Usuario')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'Administrador del sistema','admin','admin@kamerequest.local','Administrador','Activo','a94z9en0','LImV7Zp8XrcTuaeq','fsjP6/rrgVl728E0uHH+fQ==',0),(2,'Luis Rodriguez Alegria','luale','luisbasques460@gmail.com','Administrador','Activo','MIzIWlDG','iNWF1VfJtYPASKOd','iUkTXECidILOHtJOX8igbg==',0);
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-03 11:50:59
