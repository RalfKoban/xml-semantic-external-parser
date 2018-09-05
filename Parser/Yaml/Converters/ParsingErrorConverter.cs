using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml.Converters
{
    public sealed class ParsingErrorConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(ParsingError);

        public object ReadYaml(IParser parser, Type type) => throw new NotImplementedException();

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (value is ParsingError error)
            {
                emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));

                // location
                emitter.Emit(new Scalar("location"));

                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
                emitter.Emit(new Scalar(error.Location.LineNumber.ToString()));
                emitter.Emit(new Scalar(error.Location.LinePosition.ToString()));
                emitter.Emit(new SequenceEnd());

                // message
                emitter.Emit(new Scalar("message"));
                emitter.Emit(new Scalar(null, null, error.ErrorMessage, ScalarStyle.SingleQuoted, false, true));

                emitter.Emit(new MappingEnd());
            }
            else
            {
                throw new NotImplementedException("wrong type");
            }
        }
    }
}