using CadastroClienteAPI.Domain;
using CadastroClienteAPI.Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClienteAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RelacionamentoClienteController : Controller
  {
    private readonly Pessoa _pessoa;
    public RelacionamentoClienteController(Pessoa pessoa)
    {
      _pessoa = pessoa; 
    }

    [HttpPost("listarPessoas")]
    public IActionResult Cliente([FromBody] DtoPessoa pessoa)
    {
      return Ok(_pessoa.LocalicarListaRelacionamentoPessoa(pessoa.CPF));
    }

    [HttpPost("listarGrauRelacionamento")]
    public IActionResult Relacionamento()
    {
      return Ok(_pessoa.ListarRelacionamento());
    }

    [HttpPost("cadastrarRelacionamento")]
    public IActionResult RelacionarCliente([FromBody] DtoPessoaRelacionamento pessoa)
    {
      return Ok(_pessoa.CadastroPessoaRelacionamento(pessoa));
    }
  }
}
