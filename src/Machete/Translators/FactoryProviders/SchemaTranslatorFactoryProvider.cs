﻿namespace Machete.Translators.FactoryProviders
{
    using TranslateConfiguration;


    public class SchemaTranslatorFactoryProvider<TSchema> :
        ITranslatorFactoryProvider<TSchema>
        where TSchema : Entity
    {
        public ITranslatorFactory<TSchema> GetTranslateFactory(ITranslatorSpecification<TSchema> specification)
        {
            return new TranslatorFactory<TSchema>(specification);
        }
    }
}