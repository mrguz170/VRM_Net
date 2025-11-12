
-- MYSQL

USE mi_prueba;  -- El nombre de la BD

drop table cat_users;
CREATE TABLE cat_users (
	user_id VARCHAR(100) NOT NULL DEFAULT (UUID()) ,
	email varchar(150), 
	user_name varchar(50),
	role_id INT,
    name varchar(150),
    last_name varchar(150),
    second_last_name varchar(150),
    password varchar(255),
	is_active bool not null default 0,
    created_date datetime DEFAULT CURRENT_TIMESTAMP,
    updated_date datetime NULL,
    created_user_id INT NOT NULL,
    updated_user_id INT NULL,
    FOREIGN KEY (role_id) REFERENCES cat_roles(role_id),
    PRIMARY KEY(role_id,email,user_name)
);

INSERT INTO mi_prueba.cat_users (email,user_name,role_id,name,last_name,second_last_name,password, -- En producción, usar BCrypt o similar
created_user_id
) VALUES 
(
    'admin@vrm.com',
    'BetoM',
    '1',
    'Alberto', 'Martinez', 'Ozornio',
    'rlsK5kLWWPPRZwVWNhlAr3CiqxpXuHXFFNKzZbhA+VDS3pk5X5nND2RM0lau46DV',1 -- Password: Admin123!
),
(
    'gerente.finanzas@vrm.com',
    'GustavoB',
	'2',
    'Gustavo','Bañuelos', 'Ochoa',
    'rlsK5kLWWPPRZwVWNhlAr3CiqxpXuHXFFNKzZbhA+VDS3pk5X5nND2RM0lau46DV',1 -- Password: Admin123!
),
(
    'contador@vrm.com',
    'IrvingF',
	'3',
    'Irving', 'Flores', 'Marañon',
    'rlsK5kLWWPPRZwVWNhlAr3CiqxpXuHXFFNKzZbhA+VDS3pk5X5nND2RM0lau46DV',1 -- Password: Admin123!
);

drop table cat_modulEs;
CREATE TABLE cat_modules (
    module_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    module_name VARCHAR(100) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    description VARCHAR(300) NOT NULL,
    version VARCHAR(100) NOT NULL,
    is_active bool not null default 0,
    created_date datetime DEFAULT CURRENT_TIMESTAMP,
    updated_date datetime NULL,
    created_user_id INT NOT NULL,
    updated_user_id INT NULL   
);

INSERT INTO cat_modules (module_name,
    display_name,
    description,
    version,
    updated_date,
    created_user_id,
    updated_user_id) values
('Finanzas','Gestión de Finanzas',
'Módulo para gestionar operaciones financieras. Incluye facturas, pagos, conciliaciones y cuentas por pagar.',
'1.0.0',CURRENT_TIMESTAMP,1,1),
('Prospectos','Gestión de Finanzas',
'Módulo para gestionar solicitudes de proveedores. Permite recibir, revisar y aprobar empresas que desean ser proveedores.',
'1.0.0',CURRENT_TIMESTAMP,1,1);

drop table cat_components;
CREATE TABLE cat_components (
    component_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    component_name VARCHAR(100) NOT NULL,
    module_id INT NOT NULL,
    parent_id INT NULL,
    description VARCHAR(500) NOT NULL,
    route VARCHAR(300) NOT NULL,
    component_type VARCHAR(100) NULL,
    icon VARCHAR(100) NOT NULL,
    show_in_menu BIT NOT NULL DEFAULT 0,
    menu_order INT NOT NULL ,
    is_active bool not null default 0,
    created_date datetime DEFAULT CURRENT_TIMESTAMP,
    updated_date datetime NULL,
    created_user_id INT NOT NULL,
    updated_user_id INT NULL    
);

INSERT INTO cat_components (component_name,module_id,description,Route,menu_order,icon,created_user_id) values
('Finanzas',1,'Módulo principal de finanzas','',20,'ri-money-dollar-circle-line',1);
INSERT INTO cat_components (component_name,module_id,parent_id,description,route,component_type,icon,menu_order,created_user_id) values
('Facturas',1,1,'Gestión de facturas','/finanzas/facturas','Components.Facturas','ri-file-list-3-line',1,1),
('Cobros y Pagos',1,1,'Gestión de cobros y pagos','/finanzas/cobros-pagos','Components.CobrosYPagos','ri-exchange-dollar-line',2,1);

drop table cat_actions;
CREATE TABLE cat_actions(
    action_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    action_name VARCHAR(100) NOT NULL,
    description VARCHAR(100) NOT NULL,
    is_active bool not null default 0,
    created_date datetime DEFAULT CURRENT_TIMESTAMP,
    updated_date datetime NULL,
    created_user_id INT NOT NULL,
    updated_user_id INT NULL    
);

INSERT INTO cat_actions (action_name,description,created_user_id) values
('Detalles','Ver detalles',1),
('Editar','Accion para editar campos',1),
('TimbrarSAT','Accion para timbrar al SAT',1),
('Eliminar','',1),
('ExportarSensibles','',1),
('VerSensibles','',1),
('VerGenerales','',1);

drop table cat_roles;

CREATE TABLE cat_roles (
  role_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  role_name varchar(100) NOT NULL,
  description VARCHAR(300) NULL,
  is_active bool not null default 0,
  created_date datetime DEFAULT CURRENT_TIMESTAMP,
  updated_date datetime NULL,
  created_user_id INT NOT NULL,
  updated_user_id INT NULL
);

insert into cat_roles (role_name,description,created_user_id) values
('Admin','Acceso completo del sistema',1),
('Contador','Gerente de contaduria',1),
('GerenteFinanzas','Coordinador de Finanzas',1);

drop table info_actions_key;

CREATE TABLE info_actions_key (
  action_key_id BIGINT UNSIGNED NOT NULL DEFAULT (UUID_SHORT()),
  module_id int NOT NULL,
  component_id int NOT NULL,
  action_id int NOT NULL,
  is_critical bool not null default 0,
  description varchar(100) DEFAULT NULL,
  is_active bool not null default 0,
  created_date datetime DEFAULT CURRENT_TIMESTAMP,
  updated_date datetime NULL,
  created_user_id INT NOT NULL,
  updated_user_id INT NULL,
  FOREIGN KEY (module_id) REFERENCES cat_modules(module_id), 
  FOREIGN KEY (component_id) REFERENCES cat_components(component_id),
  FOREIGN KEY (action_id) REFERENCES cat_actions(action_id), 
  PRIMARY KEY (module_id,component_id,action_id),
  CONSTRAINT unique_action_key_id UNIQUE (action_key_id)
);


INSERT INTO info_actions_key (module_id,component_id,action_id,description,created_user_id) values
(1,2,8,'Crear nueva factura',1);

drop table info_actions_roles;
CREATE table info_actions_roles(
action_key_id BIGINT UNSIGNED NOT NULL,
role_id int NOT NULL ,
is_active bool not null default 0,
created_date datetime DEFAULT CURRENT_TIMESTAMP,
updated_date datetime NULL,
created_user_id INT NOT NULL,
updated_user_id INT NULL,
FOREIGN KEY (role_id) REFERENCES cat_roles(role_id), 
FOREIGN KEY (action_key_id) REFERENCES info_actions_key(action_key_id),
PRIMARY KEY (action_key_id,role_id)
);

insert into info_actions_roles (action_key_id,role_id,created_user_id) values
 ('101633185632223234',1,1)