using CadastroClienteAPI.Data;
using CadastroClienteAPI.Domain.Dto;
using CadastroClienteAPI.Domain.Enum;
using CadastroClienteAPI.Domain.ModelView;
using Dapper;

namespace CadastroClienteAPI.Domain
{
  public class Pessoa
  {
    private readonly Context _context;
    public Pessoa(Context context)
    {
      _context = context;
    }

    #region ModelView
    public ModelViewLocalizaPessoa LocalicarPessoaModelView(string cpf)
    {
      var localizandoPessoa = new ModelViewLocalizaPessoa();
      using (var con = _context.Connection())
      {
        localizandoPessoa.Pessoa = con.QueryFirstOrDefault<DtoPessoa>(@"SELECT top 1
                                                        [CD_PESSOA] as Codigo
                                                          ,[NM_PESSOA] as Nome
                                                          ,[NM_SOBRENOME] as Sobrenome
                                                          ,[NR_CPF] as CPF
                                                          ,[EMAIL] as Email
                                                          ,[FL_ATIVO] as Ativo
                                                          ,[DT_INSERCAO] as DtInsercao
                                                FROM [PESSOA]
                                                where NR_CPF = @CPFValue", new { CPFValue = cpf });
        if (localizandoPessoa != null)
        {
          localizandoPessoa.Retorno = "Pessoa Localizada";
        }
        else
        {
          localizandoPessoa.Retorno = "Pessoa nao Localizada";
        }
      }
      return localizandoPessoa;
    }

    public ModelViewResponsePessoa CadastroPessoa(DtoPessoa pessoa)
    {
      var model = new ModelViewResponsePessoa();
      var validacaoCPF = new DtoCPF();
      var localizarPessoa = new DtoPessoa();

      if (!pessoa.isRelacionamento)
      {
        validacaoCPF = ValidadorDeCPF(pessoa.CPF);

        if (validacaoCPF.Retorno == "CPF Invalido")
        {
          model.Retorno = validacaoCPF.Retorno;
          return model;
        }

        localizarPessoa = LocalicarPessoa(pessoa);

        if (localizarPessoa == null)
        {
          localizarPessoa = new DtoPessoa();
        }
      }

      if (localizarPessoa.CPF == null)
      {
        //se nao localizar pessoa sera um cadastro novo de cliente
        using (var con = _context.Connection())
        {
          con.Execute("insert into PESSOA([NM_PESSOA],[NM_SOBRENOME],[NR_CPF],[EMAIL]) values (@nome, @sobrenome, @cpf, @email)", new
          {
            nome = pessoa.Nome,
            sobrenome = pessoa.Sobrenome,
            cpf = pessoa.CPF,
            email = pessoa.Email
          });
        }

        var checarPessoaInserida = LocalicarPessoa(pessoa);
        pessoa.Codigo = checarPessoaInserida.Codigo;

        CadastrarTelefonePessoa(pessoa);
        model.Retorno = "Cadastro Realizado com Sucesso";
      }
      else
      {
        model.Retorno = "Cliente localizado, Dados incompletos para cadastro";
      }
      // verificar relacionamento e retorna para front caso exista dados incompletos

      var localizarPessoasRelacionadas = LocalicarListaRelacionamentoPessoa(pessoa.CPF);
      var localizarTelefonePessoa = ListarTelefonePessoa(LocalicarPessoa(pessoa).Codigo);

      model.Pessoa = LocalicarPessoa(pessoa);
      model.Pessoa.isRelacionamento = localizarPessoasRelacionadas.Any();
      model.Pessoa.DDD = (localizarTelefonePessoa.Count == 0) ? model.Pessoa.DDD : localizarTelefonePessoa.FirstOrDefault().DDD.ToString();
      model.Pessoa.Telefone = (localizarTelefonePessoa.Count == 0) ? model.Pessoa.Telefone : localizarTelefonePessoa.FirstOrDefault().NumeroTelefone.ToString();
      model.Pessoa.TipoTelefone = (localizarTelefonePessoa.Count == 0) ? model.Pessoa.TipoTelefone : (int)localizarTelefonePessoa.FirstOrDefault().TipoTelefoneEnum;

      return model;
    }

    public ModelViewResponsePessoa CadastroPessoaRelacionamento(DtoPessoaRelacionamento pessoaR)
    {
      var model = new ModelViewResponsePessoa();

      var pessoa = LocalicarPessoaRelacionamento(pessoaR.PessoaRelacionada.FirstOrDefault());

      var localizarListaRelacionamento = LocalicarListaRelacionamentoPessoa(pessoaR.PessoaRelacionada.FirstOrDefault().CPF);

      if (pessoa != null || localizarListaRelacionamento.Count > 0)
      {
        model.Retorno = "Cliente ja existente ou ja Relacionado";
      }
      else // se não localizar adiciona novo cliente na tabela Pessoa e cria um relacionamento com codigoCliente informado
      {
        var newPessoa = pessoaR.PessoaRelacionada.FirstOrDefault();
        newPessoa.isRelacionamento = true;
        var cadastrarPessoa = CadastroPessoa(newPessoa);

        if (cadastrarPessoa.Retorno == "Cadastro Realizado com Sucesso")
        {
          using (var con = _context.Connection())
          {
            con.Execute("insert into [RELACIONAMENTO_PESSOA](CD_PESSOA,CD_PESSOA_RELACIONAMENTO, NR_TIPO_RELACIONAMENTO_PESSOA) values(@CDPESSOA, @CDPESSOARELACIONAMENTO, @NRTIPORELACIONAMENTOPESSOA)", new
            {
              CDPESSOA = pessoaR.CodigoCliente,
              CDPESSOARELACIONAMENTO = cadastrarPessoa.Pessoa.Codigo,
              NRTIPORELACIONAMENTOPESSOA = 1
            });

            model.Retorno = "Relacionamento criaodo com Sucesso";
          }
        }

      }

      return model;
    }
    #endregion ModelView

    #region Listas
    public List<DtoTipoTelefone> ListarTiposTelefones()
    {
      using (var con = _context.Connection())
      {
        return con.Query<DtoTipoTelefone>(@"SELECT
                                              [NR_TIPO_TELEFONE] as NrTipoTelefone
                                              ,[NM_TIPO_TELEFONE] as TipoTelefone
                                           FROM[TIPO_TELEFONE]").ToList();

      }
    }

    public List<DtoTelefone> ListarTelefonePessoa(int cdPessoa)
    {
      using (var con = _context.Connection())
      {
        return con.Query<DtoTelefone>(@"Select 
                                        CD_PESSOA as CodigoPessoa
                                        ,DDD as DDD
                                        ,TELEFONE as NumeroTelefone
                                        ,NR_TIPO_TELEFONE  as TipoTelefoneEnum
                                        from 
	                                        TELEFONE_PESSOA where CD_PESSOA = @CDPESSOAValue", new { CDPESSOAValue = cdPessoa }).ToList();

      }
    }

    public List<DtoTelefone> AtualizarDadosPessoa(int cdPessoa)
    {
      using (var con = _context.Connection())
      {
        return con.Query<DtoTelefone>(@"Select 
                                        CD_PESSOA as CodigoPessoa
                                        ,DDD as DDD
                                        ,TELEFONE as NumeroTelefone
                                        ,NR_TIPO_TELEFONE  as TipoTelefoneEnum
                                        from 
	                                        TELEFONE_PESSOA where CD_PESSOA = @CDPESSOAValue", new { CDPESSOAValue = cdPessoa }).ToList();

      }
    }

    public List<DtoPessoa> LocalicarListaRelacionamentoPessoa(string cpf)
    {

      using (var con = _context.Connection())
      {
        return con.Query<DtoPessoa>(@"SELECT 
                                          RP.CD_PESSOA_RELACIONAMENTO AS Codigo
	                                      ,PR.NM_PESSOA AS Nome
	                                      ,PR.NM_SOBRENOME AS Sobrenome
	                                      ,PR.NR_CPF AS CPF
	                                      ,TP.DDD AS DDD
	                                      ,TP.TELEFONE AS Telefone
	                                      ,TT.NR_TIPO_TELEFONE AS TipoTelefone
                                      FROM 
                                      RELACIONAMENTO_PESSOA	    RP
                                      JOIN PESSOA				P	ON RP.CD_PESSOA					= P.CD_PESSOA 
                                      JOIN PESSOA				PR	ON RP.CD_PESSOA_RELACIONAMENTO  = PR.CD_PESSOA
                                      JOIN TELEFONE_PESSOA		TP	ON TP.CD_PESSOA					= RP.CD_PESSOA_RELACIONAMENTO
                                      JOIN TIPO_TELEFONE		TT	ON TT.NR_TIPO_TELEFONE			= TP.NR_TIPO_TELEFONE
                                      WHERE
                                      P.NR_CPF = @CPFValue", new { CPFValue = cpf }).OrderBy(x=> x.Codigo).ToList();

      }

    }

    public List<DtoGrauRelacionamento> ListarRelacionamento()
    {
      using (var con = _context.Connection())
      {
        return con.Query<DtoGrauRelacionamento>(@"SELECT
                                                    [NR_TIPO_RELACIONAMENTO_PESSOA] as NrTipoGrauRelacionamento
                                                    ,[NM_TIPO_RELACIONAMENTO_PESSOA] as GrauRelacionamento
                                                  FROM 
	                                                [TIPO_RELACIONAMENTO_PESSOA]").ToList();
      }
    }
    #endregion Listas

    #region Voids
    private void CadastrarTelefonePessoa(DtoPessoa pessoa)
    {
      using (var con = _context.Connection())
      {
        con.Execute("INSERT INTO [TELEFONE_PESSOA]([CD_PESSOA],[DDD],[TELEFONE],[NR_TIPO_TELEFONE])VALUES(@CDPESSOAVALUE,@DDDVALUE,@TELEFONEVALUE,@NRTIPOTELEFONEENUMVALUE)", new
        {
          CDPESSOAVALUE = pessoa.Codigo,
          DDDVALUE = pessoa.DDD,
          TELEFONEVALUE = pessoa.Telefone,
          NRTIPOTELEFONEENUMVALUE = pessoa.TipoTelefone
        });

      }
    }
    #endregion Voids

    #region Dto
    private DtoCPF ValidadorDeCPF(string cpf)
    {
      DtoCPF retorno = new DtoCPF();

      int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };

      int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

      string tempCpf;

      string digito;

      int soma;

      int resto;

      cpf = cpf.Trim();

      cpf = cpf.Replace(".", "").Replace("-", "");

      if (cpf.Length != 11)
      {
        retorno.Retorno = "CPF Invalido";
        return retorno;
      }

      tempCpf = cpf.Substring(0, 9);

      soma = 0;

      for (int i = 0; i < 9; i++)
      {
        soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
      }

      resto = soma % 11;

      if (resto < 2)
      {
        resto = 0;
      }
      else
      {
        resto = 11 - resto;
      }

      digito = resto.ToString();

      tempCpf = tempCpf + digito;

      soma = 0;

      for (int i = 0; i < 10; i++)
      {
        soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
      }

      resto = soma % 11;

      if (resto < 2)
      {
        resto = 0;
      }
      else
      {
        resto = 11 - resto;
      }

      digito = digito + resto.ToString();

      var cpfValido = cpf.EndsWith(digito);

      if (cpfValido)
      {
        retorno.Retorno = "CPF Valido";
      }
      else
      {
        retorno.Retorno = "CPF Invalido";
      }

      return retorno;

    }

    public DtoPessoa LocalicarPessoa(DtoPessoa pessoa)
    {
      var pessoaR = new DtoPessoa();
      using (var con = _context.Connection())
      {
        pessoaR = con.QueryFirstOrDefault<DtoPessoa>(@"SELECT top 1
                                                        [CD_PESSOA] as Codigo
                                                          ,[NM_PESSOA] as Nome
                                                          ,[NM_SOBRENOME] as Sobrenome
                                                          ,[NR_CPF] as CPF
                                                          ,[EMAIL] as Email
                                                          ,[FL_ATIVO] as Ativo
                                                          ,[DT_INSERCAO] as DtInsercao
                                                FROM [PESSOA]
                                                where NR_CPF = @CPFValue", new { CPFValue = pessoa.CPF });

        if (pessoaR.CPF == "")
        {
          pessoaR = con.QueryFirstOrDefault<DtoPessoa>(@"SELECT top 1
                                                      [CD_PESSOA] as Codigo
                                                      ,[NM_PESSOA] as Nome
                                                      ,[NM_SOBRENOME] as Sobrenome
                                                      ,[NR_CPF] as CPF
                                                      ,[EMAIL] as Email
                                                      ,[FL_ATIVO] as Ativo
                                                      ,[DT_INSERCAO] as DtInsercao
                                                      FROM [PESSOA]
                                                      where [NM_PESSOA] = @NomeValue ", new { NomeValue = pessoa.Nome });
        }


      }
      return pessoaR;
    }

    public DtoPessoa LocalicarPessoaRelacionamento(DtoPessoa pessoa)
    {
      var pessoaR = new DtoPessoa();
      using (var con = _context.Connection())
      {
        if (pessoa.CPF != "")
        {

          pessoaR = con.QueryFirstOrDefault<DtoPessoa>(@"SELECT top 1
                                                          [CD_PESSOA] as Codigo
                                                            ,[NM_PESSOA] as Nome
                                                            ,[NM_SOBRENOME] as Sobrenome
                                                            ,[NR_CPF] as CPF
                                                            ,[EMAIL] as Email
                                                            ,[FL_ATIVO] as Ativo
                                                            ,[DT_INSERCAO] as DtInsercao
                                                  FROM [PESSOA]
                                                  where NR_CPF = @CPFValue", new { CPFValue = pessoa.CPF });
        }

        pessoaR = con.QueryFirstOrDefault<DtoPessoa>(@"SELECT top 1
                                                      [CD_PESSOA] as Codigo
                                                      ,[NM_PESSOA] as Nome
                                                      ,[NM_SOBRENOME] as Sobrenome
                                                      ,[NR_CPF] as CPF
                                                      ,[EMAIL] as Email
                                                      ,[FL_ATIVO] as Ativo
                                                      ,[DT_INSERCAO] as DtInsercao
                                                      FROM [PESSOA]
                                                      where [NM_PESSOA] = @NomeValue ", new { NomeValue = pessoa.Nome });

        if (pessoaR != null)
        {
          var telefonePessoa = con.Query<DtoTelefone>(@"Select 
                                                        CD_PESSOA as CodigoPessoa
                                                        ,DDD as DDD
                                                        ,TELEFONE as NumeroTelefone
                                                        ,NR_TIPO_TELEFONE  as TipoTelefoneEnum
                                                        from 
	                                                     TELEFONE_PESSOA where CD_PESSOA = @CDPESSOAValue", new { CDPESSOAValue = pessoa.Codigo }).ToList();

          if (telefonePessoa.Count > 0)
          {
            pessoaR.DDD = telefonePessoa.FirstOrDefault().DDD.ToString();
            pessoaR.Telefone = telefonePessoa.FirstOrDefault().NumeroTelefone;
          var tt = con.QueryFirst<DtoGrauRelacionamento>(@"").NrTipoGrauRelacionamento;
          }
          //pessoaR.TipoRelacionamento = 
        }


      }
      return pessoaR;
    }

    #endregion Dto

  }
}
