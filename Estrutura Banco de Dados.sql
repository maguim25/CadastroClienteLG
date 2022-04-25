  USE TesteLG
  GO 
  
CREATE TABLE PESSOA(
  CD_PESSOA INT IDENTITY(1,1),
  NM_PESSOA VARCHAR(1000) NOT NULL,
  NM_SOBRENOME VARCHAR(255),
  NR_CPF VARCHAR(255),
  EMAIL  VARCHAR(350),
  FL_ATIVO BIT DEFAULT 1 NOT NULL,
  DT_INSERCAO DATETIME DEFAULT GETDATE() NOT NULL
  CONSTRAINT PK_PESSOA PRIMARY KEY(CD_PESSOA)
)

CREATE TABLE RELACIONAMENTO_PESSOA(
  CD_RELACIONAMENTO_PESSOA INT IDENTITY(1,1),
  CD_PESSOA INT NOT NULL,
  CD_PESSOA_RELACIONAMENTO INT NOT NULL,
  DT_INSERCAO DATETIME DEFAULT GETDATE() NOT NULL
  CONSTRAINT PK_RELACIONAMENTO_PESSOA PRIMARY KEY(CD_RELACIONAMENTO_PESSOA)
)

CREATE TABLE TELEFONE_PESSOA(
  CD_TELEFONE_PESSOA INT IDENTITY(1,1),
  CD_PESSOA INT NOT NULL,
  DDD INT NOT NULL,
  TELEFONE VARCHAR(15) NOT NULL,
  NR_TIPO_TELEFONE INT NOT NULL
  CONSTRAINT PK_TELEFONE_PESSOA PRIMARY KEY(CD_TELEFONE_PESSOA)
)

CREATE TABLE TIPO_TELEFONE(
  NR_TIPO_TELEFONE INT IDENTITY(1,1),
  NM_TIPO_TELEFONE VARCHAR(50) NOT NULL,
  CONSTRAINT PK_TIPO_TELEFONE PRIMARY KEY(NR_TIPO_TELEFONE)
)

 ALTER TABLE RELACIONAMENTO_PESSOA
	ADD CONSTRAINT FK_RELACIONAMENTO_PESSOA FOREIGN KEY (CD_PESSOA)
	REFERENCES PESSOA (CD_PESSOA)

 ALTER TABLE TELEFONE_PESSOA
	ADD CONSTRAINT FK_TELEFONE_PESSOA FOREIGN KEY (CD_PESSOA)
	REFERENCES PESSOA (CD_PESSOA)



insert into [TIPO_TELEFONE] ([NM_TIPO_TELEFONE]) values ( 'Celular')
insert into [TIPO_TELEFONE] ([NM_TIPO_TELEFONE]) values ( 'Residencial')
insert into [TIPO_TELEFONE] ([NM_TIPO_TELEFONE]) values ( 'Comercial')




   --ALTER TABLE RELACIONAMENTO_PESSOA DROP CONSTRAINT FK_RELACIONAMENTO_PESSOA

   --DROP TABLE PESSOA
