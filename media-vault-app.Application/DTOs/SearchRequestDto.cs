using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs
{
    public record SearchRequestDto(string Query, int Page);

}