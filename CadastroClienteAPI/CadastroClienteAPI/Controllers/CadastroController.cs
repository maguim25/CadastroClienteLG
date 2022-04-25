using CadastroClienteAPI.Domain;
using CadastroClienteAPI.Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClienteAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class CadastroController : Controller
  {
    private readonly Pessoa _pessoa;
    public CadastroController(Pessoa pessoa)
    {
      _pessoa = pessoa;
    }

    [HttpPost("cliente")]
    public IActionResult Cliente([FromBody] DtoPessoa pessoa)
    {
      return Ok(_pessoa.CadastroPessoa(pessoa));
    }

    [HttpPost("listarTipoTelefone")]
    public IActionResult TipoTelefone()
    {
      return Ok(_pessoa.ListarTiposTelefones());
    }

  }
}
