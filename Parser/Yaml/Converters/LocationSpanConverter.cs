using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml.Converters
{
    public sealed class LocationSpanConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(LocationSpan);

        public object ReadYaml(IParser parser, Type type) => throw new NotImplementedException();

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var span = value as LocationSpan;
            if (span == null)
            {
                throw new NotImplementedException("wrong type");
            }

            emitter.Emit(new MappingStart(null, null, true, MappingStyle.Flow));

            // start
            emitter.Emit(new Scalar("start"));

            emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));
            emitter.Emit(new Scalar(span.Start.LineNumber.ToString()));
            emitter.Emit(new Scalar(span.Start.LinePosition.ToString()));
            emitter.Emit(new SequenceEnd());

            // end
            emitter.Emit(new Scalar("end"));

            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
            emitter.Emit(new Scalar(span.End.LineNumber.ToString()));
            emitter.Emit(new Scalar(span.End.LinePosition.ToString()));
            emitter.Emit(new SequenceEnd());

            emitter.Emit(new MappingEnd());
        }
    }
}