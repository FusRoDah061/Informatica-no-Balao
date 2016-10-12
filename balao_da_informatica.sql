drop database if exists balao_da_informatica;
create database balao_da_informatica;

use balao_da_informatica;

create table produtos(codigo int(3) AUTO_INCREMENT NOT NULL ,
                      preco_unit double(8,2) NOT NULL,
                      nome varchar(100) NOT NULL,
                      tipo varchar(50) NOT NULL,
                      PRIMARY KEY(codigo));

create table clientes(nome varchar(50) NOT NULL,
                      rg varchar(12) NOT NULL,
                      cpf varchar(14) NOT NULL,
                      email varchar(50),
                      telefone varchar(12) NOT NULL,
                      celular varchar(12),
                      operador_responsavel varchar(5) NOT NULL);

create table localizacoes(pessoa_cpf varchar(14) NOT NULL,
                          bairro varchar(20) NOT NULL,
                          endereco varchar(100) NOT NULL,
                          cidade varchar(20) NOT NULL, 
                          cep varchar(9) NOT NULL,
                          estado varchar(2) NOT NULL);

create table compras(codigo int(3) AUTO_INCREMENT NOT NULL ,
                     operador varchar(5) NOT NULL,
                     cliente_nome varchar(50) NOT NULL,
                     cliente_cpf varchar(14) NOT NULL,
                     codigo_produto varchar(38) NOT NULL,
                     valor_final double(8,2) NOT NULL,
                     PRIMARY KEY(codigo));
                     
create table funcionarios(id varchar(5) NOT NULL,
                          nome varchar(50) NOT NULL,
                          rg varchar(12) NOT NULL,
                          cpf varchar(14) NOT NULL,
                          email varchar(50),
                          telefone varchar(12) NOT NULL,
                          celular varchar(12));

delete from produtos;

insert into produtos(preco_unit, nome, tipo) values (168.00, 'Intel Celeron G1820', 'processador');
insert into produtos(preco_unit, nome, tipo) values (485.00, 'AMD FX-6300 Box Black Edition', 'processador');
insert into produtos(preco_unit, nome, tipo) values (599.00, 'Intel 4170 Core i3', 'processador');
insert into produtos(preco_unit, nome, tipo) values (1255.00, 'Intel Core I5-4690K', 'processador');
insert into produtos(preco_unit, nome, tipo) values (1999.00, 'Intel Core i7-6700K', 'processador');


insert into produtos(preco_unit, nome, tipo) values (538.00, 'Asrock H87M', 'placamae');
insert into produtos(preco_unit, nome, tipo) values (647.90, 'Asus Z97M-PLUS/BR', 'placamae');
insert into produtos(preco_unit, nome, tipo) values (699.00, 'ASUS Z170-P D3', 'placamae');
insert into produtos(preco_unit, nome, tipo) values (1151.90, 'Asus Sabertooth Mark 2', 'placamae');
insert into produtos(preco_unit, nome, tipo) values (2999.00, 'ASUS Maximus VIII Extreme Z170', 'placamae');


insert into produtos(preco_unit, nome, tipo) values (199.00, 'VGA Gigabyte GeForce GT 420', 'placavideo');
insert into produtos(preco_unit, nome, tipo) values (679.00, 'VGA EVGA GeForce GTX950', 'placavideo');
insert into produtos(preco_unit, nome, tipo) values (725.00, 'Gigabyte GeForce GPU GTX 940', 'placavideo');
insert into produtos(preco_unit, nome, tipo) values (1287.00, 'EVGA GeForce Nvidia GTX 970', 'placavideo');
insert into produtos(preco_unit, nome, tipo) values (2999.00, 'Gigabyte GeForce GTX 1070 Founders Edition', 'placavideo');


insert into produtos(preco_unit, nome, tipo) values (109.00, 'Kingston 4GB', 'memoria');
insert into produtos(preco_unit, nome, tipo) values (188.00, 'Kingston 8GB', 'memoria');
insert into produtos(preco_unit, nome, tipo) values (199.90, 'Corsair Gaming 8GB', 'memoria');
insert into produtos(preco_unit, nome, tipo) values (199.99, 'Corsair Vengeance 8GB', 'memoria');
insert into produtos(preco_unit, nome, tipo) values (210.00, 'Kingston HyperX Fury 8GB', 'memoria');


insert into produtos(preco_unit, nome, tipo) values (131.00, 'BR ONE 530w', 'fonte');
insert into produtos(preco_unit, nome, tipo) values (137.00, 'Gamemax 500w', 'fonte');
insert into produtos(preco_unit, nome, tipo) values (162.00, 'BR ONE 600w','fonte');
insert into produtos(preco_unit, nome, tipo) values (1031.00, 'EVGA 80PLUS Gold 1000w', 'fonte');
insert into produtos(preco_unit, nome, tipo) values (1519.00, 'Corsair 1.200W Platinum AX1200i Digital', 'fonte');


insert into produtos(preco_unit, nome, tipo) values (256.00, 'Seagate 1TB', 'hd');
insert into produtos(preco_unit, nome, tipo) values (318.00, 'HGST Travelstar Z5K500 (Notebook)', 'hd');
insert into produtos(preco_unit, nome, tipo) values (422.00, 'Seagate Barracuda 2TB', 'hd');
insert into produtos(preco_unit, nome, tipo) values (531.00, 'Seagate 3TB', 'hd');
insert into produtos(preco_unit, nome, tipo) values (671.00, 'Western Digital 3TB', 'hd');


insert into produtos(preco_unit, nome, tipo) values (313.00, 'LED AOC 15.6 Widescreen', 'monitor');
insert into produtos(preco_unit, nome, tipo) values (361.00, 'AOC 18.5 LED Widescreen', 'monitor');
insert into produtos(preco_unit, nome, tipo) values (363.00, 'LG LED 15.6 Widescreen', 'monitor');
insert into produtos(preco_unit, nome, tipo) values (554.74, 'AOC LED 21.5 Widescreen', 'monitor');
insert into produtos(preco_unit, nome, tipo) values (662.00, 'LG LED 23 Widescreen', 'monitor');


insert into produtos(preco_unit, nome, tipo) values (32.90, 'Slim Multimídia Multilaser TC071', 'teclado');
insert into produtos(preco_unit, nome, tipo) values (64.72, 'Logitech K120', 'teclado');
insert into produtos(preco_unit, nome, tipo) values (76.70, 'C3 Tech K-W500BK s/fio', 'teclado');
insert into produtos(preco_unit, nome, tipo) values (127.00, 'Multilaser Lightning TC195', 'teclado');
insert into produtos(preco_unit, nome, tipo) values (128.16, 'Shadow Hunter Macro HL-SHKM Hardline', 'teclado');


insert into produtos(preco_unit, nome, tipo) values (10.91, 'Óptico Multilaser MO030', 'mouse');
insert into produtos(preco_unit, nome, tipo) values (36.74, 'Óptico Logitech M100', 'mouse');
insert into produtos(preco_unit, nome, tipo) values (38.64, 'Predador Evus MG-03', 'mouse');
insert into produtos(preco_unit, nome, tipo) values (75.90, 'Pro Laser Multilaser MO191', 'mouse');
insert into produtos(preco_unit, nome, tipo) values (241.21, 'Thermaltake Laser Sports', 'mouse');


insert into produtos(preco_unit, nome, tipo) values (97.00, 'W8 Hardline', 'gabinete');
insert into produtos(preco_unit, nome, tipo) values (304.00, 'Corsair Carbide Spec-01', 'gabinete');
insert into produtos(preco_unit, nome, tipo) values (722.39, 'Thermaltake Chaser A71 Full Tower', 'gabinete');
insert into produtos(preco_unit, nome, tipo) values (894.00, 'Thermaltake Chaser MK-I', 'gabinete');
insert into produtos(preco_unit, nome, tipo) values (991.99, 'Mid-Tower H440 Razer Edition NZXT', 'gabinete');