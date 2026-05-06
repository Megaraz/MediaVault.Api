using System;
using System.Collections.Generic;
using System.Text;

namespace media_vault_app.Application.DTOs.MediaEntry.Base_Classes.Search
{
    public record SearchRequestDto(string Query, int Page);

}