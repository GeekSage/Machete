﻿namespace Machete
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Configuration;
    using TranslateConfiguration;
    using TranslateConfiguration.Specifications;
    using Values;


    public abstract class EntityTranslation<TResult, TInput, TSchema> :
        IEntityTranslatorSpecification<TResult, TInput, TSchema>
        where TSchema : Entity
        where TInput : TSchema
        where TResult : TSchema
    {
        readonly EntityTranslatorSpecification<TResult, TInput, TSchema> _specification;

        protected EntityTranslation()
        {
            _specification = new EntityTranslatorSpecification<TResult, TInput, TSchema>();

            _specification.Name = GetType().Name;

            Exclude(x => x.IsEmpty);
            Exclude(x => x.Fields);
        }

        void IEntityTranslatorSpecification<TResult, TInput, TSchema>.Apply(IEntityTranslatorBuilder<TResult, TInput, TSchema> builder)
        {
            _specification.Apply(builder);
        }

        IEnumerable<ValidateResult> ISpecification.Validate()
        {
            return _specification.Validate();
        }

        public string Name
        {
            get => _specification.Name;
            set => _specification.Name = value;
        }

        /// <summary>
        /// Copy the value from the input to the result, using the same property name on the input as the result
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Copy<T>(Expression<Func<TResult, Value<T>>> propertyExpression)
        {
            var specification = new CopyInputValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Copy the value from the input to the result, using the same property name on the input as the result
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Copy<T>(Expression<Func<TResult, ValueList<T>>> propertyExpression)
        {
            var specification = new CopyInputValueListPropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Copy the value from the input value to the result value, allowing a specific input property to be specified. This overrides the default
        /// behavior of mapping the input property of the same name to the result.
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <param name="inputPropertyExpression">The input property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Copy<T>(Expression<Func<TResult, Value<T>>> propertyExpression, Expression<Func<TInput, Value<T>>> inputPropertyExpression)
        {
            var specification = new CopyInputValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression, inputPropertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Copy the value from the input value to the result value, allowing a specific input property to be specified. This overrides the default
        /// behavior of mapping the input property of the same name to the result.
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <param name="inputPropertyExpression">The input property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Copy<T>(Expression<Func<TResult, ValueList<T>>> propertyExpression, Expression<Func<TInput, ValueList<T>>> inputPropertyExpression)
        {
            var specification = new CopyInputValueListPropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression, inputPropertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Exclude the ValueList from the translator, setting the property to Missing
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Exclude<T>(Expression<Func<TResult, Value<T>>> propertyExpression)
        {
            var specification = new ExcludeValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Copy the value from the input to the result, using the same property name on the input as the result
        /// </summary>
        /// <param name="propertyExpression">The property reference expression</param>
        /// <typeparam name="T">The value type</typeparam>
        protected void Exclude<T>(Expression<Func<TResult, ValueList<T>>> propertyExpression)
        {
            var specification = new ExcludeValueListPropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression);

            _specification.Add(specification);
        }

        /// <summary>
        /// Set the value using the specified value provider
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <param name="valueProvider"></param>
        /// <typeparam name="T"></typeparam>
        protected void Set<T>(Expression<Func<TResult, Value<T>>> propertyExpression, SetValueProvider<TInput, TSchema, T> valueProvider)
        {
            var specification = new SetValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression, valueProvider);

            _specification.Add(specification);
        }

        /// <summary>
        /// Set the value using the specified value provider
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <param name="valueProvider"></param>
        /// <typeparam name="T"></typeparam>
        protected void Set<T>(Expression<Func<TResult, Value<T>>> propertyExpression, SetValueTypeProvider<TInput, TSchema, T> valueProvider)
            where T : struct
        {
            Value<T> ValueProvider(TranslateContext<TInput, TSchema> context)
            {
                var value = valueProvider(context);

                return new ConstantValue<T>(value);
            }

            var specification = new SetValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression, ValueProvider);

            _specification.Add(specification);
        }

        /// <summary>
        /// Set the value using the specified value provider
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <param name="valueProvider"></param>
        /// <typeparam name="T"></typeparam>
        protected void Set<T>(Expression<Func<TResult, Value<T>>> propertyExpression, SetNullableValueTypeProvider<TInput, TSchema, T> valueProvider)
            where T : struct
        {
            Value<T> ValueProvider(TranslateContext<TInput, TSchema> context)
            {
                var value = valueProvider(context);

                return new ConstantValue<T>(value ?? default, value.HasValue);
            }

            var specification = new SetValuePropertyTranslatorSpecification<TResult, T, TInput, TSchema>(propertyExpression, ValueProvider);

            _specification.Add(specification);
        }

        protected void Translate<T>(Expression<Func<TResult, Value<T>>> propertyExpression, Action<ITranslateEntityUsingConfigurator<T, TSchema>> configure)
            where T : TSchema
        {
            var specification = new TranslateEntityValueUsingConfigurator<TResult, TInput, T, TSchema>(_specification, propertyExpression);

            configure?.Invoke(specification);
        }
    }
}